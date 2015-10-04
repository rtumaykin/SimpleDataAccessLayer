using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Management.Common;

namespace SimpleDataAccessLayer.Common.codegen
{
#if DEBUG
    public class SqlRepository : ISqlRepository
#else 
    internal class SqlRepository : ISqlRepository
#endif
    {
        private readonly string _designerConnectionString;

        public SqlRepository(string designerConnectionString)
        {
            _designerConnectionString = designerConnectionString;
        }

        public IList<EnumKeyValue> GetEnumKeyValues(string objectSchemaName, string objectName, string valueColumnName, string keyColumnName)
        {
            var retValue = new List<EnumKeyValue>();
            var fullObjectName = Tools.QuoteName(objectSchemaName) + "." + Tools.QuoteName(objectName);

            using (var conn = new SqlConnection(_designerConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_executesql";

                    cmd.Parameters.Add(new SqlParameter("@stmt",
                        String.Format("SELECT CONVERT(bigint, {0}) AS [Value], {1} AS [Key] FROM {2} ORDER BY {0}",
                            valueColumnName, keyColumnName, fullObjectName)));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            retValue.Add(new EnumKeyValue((string)reader["Key"], (long)reader["Value"]));
                        }
                    }
                }
            }
            return retValue;

        }

        public IList<ProcedureParameter> GetProcedureParameterCollection(string objectSchemaName, string objectName)
        {
            var fullObjectName = Tools.QuoteName(objectSchemaName) + "." + Tools.QuoteName(objectName);
            var retValue = new List<ProcedureParameter>();

            using (var conn = new SqlConnection(_designerConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_executesql";

                    const string stmt = @"
                            SELECT 
	                            p.[name] AS ParameterName,
	                            p.[max_length] AS MaxByteLength,
	                            p.[precision] AS [Precision],
	                            p.[scale] AS Scale,
	                            p.[is_output] AS IsOutputParameter,
	                            ISNULL(st.name, t.[name]) AS TypeName,
	                            SCHEMA_NAME(t.[schema_id]) AS TypeSchemaName,
	                            t.is_table_type
                            FROM sys.[parameters] p
	                            INNER JOIN sys.[types] t
		                            ON	t.[user_type_id] = p.[user_type_id]
	                            LEFT OUTER JOIN sys.[types] st
		                            ON	st.user_type_id = p.[system_type_id]
                            WHERE p.[object_id] = OBJECT_ID(@ObjectName);
					";

                    cmd.Parameters.AddWithValue("@stmt", stmt);
                    cmd.Parameters.AddWithValue("@params", "@ObjectName sysname");
                    cmd.Parameters.AddWithValue("@ObjectName", fullObjectName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Remove @ from the beginning of the parameter
                            var parameterName = reader.GetSqlString(0).Value.Substring(1);
                            var sqlTypeName = reader.GetSqlString(5).Value;
                            int maxByteLength = reader.GetSqlInt16(1).Value;
                            var precision = reader.GetSqlByte(2).Value;
                            var scale = reader.GetSqlByte(3).Value;
                            var isOutputParameter = reader.GetSqlBoolean(4).Value;
                            var schemaName = reader.GetSqlString(6).Value;
                            var isTableType = reader.GetSqlBoolean(7).Value;

                            var clrTypeName = schemaName == "sys"
                                ? Tools.ClrTypeName(sqlTypeName)
                                : $"TableVariables.{Tools.ValidIdentifier(schemaName)}.{Tools.ValidIdentifier(sqlTypeName)}";

                            if (!string.IsNullOrWhiteSpace(clrTypeName) || isTableType)
                            {
                                retValue.Add(new ProcedureParameter(parameterName, maxByteLength, precision, scale,
                                    isOutputParameter,
                                    schemaName == "sys"
                                        ? sqlTypeName
                                        : $"{Tools.QuoteName(schemaName)}.{Tools.QuoteName(sqlTypeName)}",
                                    isTableType,
                                    clrTypeName));
                            }
                        }
                    }
                }
            }
            return retValue;
        }

        public IList<List<ProcedureResultSetColumn>> GetProcedureResultSetColumnCollection(string objectSchemaName, string objectName)
        {
            // SQL 2012 still can use FMTONLY so this is only for the higher versions 
            if (GetDatabaseCompatibilityLevel() > 110)
            {
                return GetProcedureResultSetColumnCollection2014(objectSchemaName, objectName);
            }
            else
            {
                return GetProcedureResultSetColumnCollection2005(objectSchemaName, objectName);
            }

        }

        private byte GetDatabaseCompatibilityLevel()
        {
            byte retValue;
            using (var conn = new SqlConnection(_designerConnectionString))
            {
                conn.Open();

                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "sp_executesql";
                        cmd.Parameters.AddWithValue("@stmt",
                            @"SELECT @CompatibilityLevel = [compatibility_level] FROM sys.[databases] WHERE [database_id] = DB_ID();");
                        cmd.Parameters.AddWithValue("@params", "@CompatibilityLevel tinyint OUTPUT");
                        cmd.Parameters.Add(new SqlParameter("@CompatibilityLevel", SqlDbType.TinyInt)
                        {
                            Direction = ParameterDirection.Output
                        });
                        cmd.ExecuteNonQuery();
                        retValue = (byte)cmd.Parameters["@CompatibilityLevel"].Value;
                    }
                }
                catch // (Exception e)
                {
                    retValue = 0;
                }
            }
            return retValue;
        }

        private IList<List<ProcedureResultSetColumn>> GetProcedureResultSetColumnCollection2005(string objectSchemaName,
            string objectName)
        {
            var fullObjectName = Tools.QuoteName(objectSchemaName) + "." + Tools.QuoteName(objectName);
            var retValue = new List<List<ProcedureResultSetColumn>>();
            try
            {
                var sb = new SqlConnectionStringBuilder(_designerConnectionString);
                var conn = sb.IntegratedSecurity
                    ? new ServerConnection(sb.DataSource)
                    : new ServerConnection(sb.DataSource, sb.UserID, sb.Password);
                conn.DatabaseName = sb.InitialCatalog;
                var procedureCall = "EXEC " + fullObjectName;
                var isFirstParam = true;
                foreach (var param in GetProcedureParameterCollection(objectSchemaName, objectName))
                {
                    procedureCall += (isFirstParam ? " " : ", ") + "@" + param.ParameterName + " = NULL";
                    isFirstParam = false;
                }

                var ds = conn.ExecuteWithResults(new StringCollection() { "SET FMTONLY ON;", procedureCall + ";" });
                retValue.AddRange(from DataTable dt in ds[1].Tables
                                  select
                                      (from DataColumn column in dt.Columns
                                       select new ProcedureResultSetColumn(column.ColumnName, column.DataType.FullName)).ToList());
            }
            catch
            {
                // Whatever happens just don't return anything
                return new List<List<ProcedureResultSetColumn>>();
            }
            // GetProcedureParameterCollection
            return retValue;
        }

        private IList<List<ProcedureResultSetColumn>> GetProcedureResultSetColumnCollection2014(string objectSchemaName,
            string objectName)
        {
            var firstRecordset = new List<ProcedureResultSetColumn>();
            var fullObjectName = Tools.QuoteName(objectSchemaName) + "." + Tools.QuoteName(objectName);

            using (var conn = new SqlConnection(_designerConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_executesql";
                    cmd.Parameters.AddWithValue("@stmt", @"
				SELECT 
					c.[name] AS ColumnName,
					c.[precision],
					c.[scale],
					ISNULL(t.[name], tu.[name]) AS TypeName 
				FROM sys.[dm_exec_describe_first_result_set_for_object](OBJECT_ID(@ObjectFullName), 0) c
					LEFT OUTER JOIN sys.[types] t
						ON	t.[user_type_id] = c.[system_type_id]
					LEFT OUTER JOIN sys.[types] tu
						ON	tu.[user_type_id] = c.[user_type_id]
				WHERE c.name IS NOT NULL");
                    cmd.Parameters.AddWithValue("@params", "@ObjectFullName sysname");
                    cmd.Parameters.AddWithValue("@ObjectFullName", fullObjectName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            firstRecordset.Add(new ProcedureResultSetColumn(reader.GetSqlString(0).Value,
                                Tools.ClrTypeName(reader.GetSqlString(3).Value)));

                        }
                    }
                }
            }

            return firstRecordset.Count > 0
                ? new List<List<ProcedureResultSetColumn>> { firstRecordset }
                : new List<List<ProcedureResultSetColumn>>();
        }


        public IList<TableTypeColumn> GetTableTypeColumns(TableType tableType)
        {
            var retValue = new List<TableTypeColumn>();

            using (var conn = new SqlConnection(_designerConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_executesql";

                    const string stmt =
                        "SELECT " +
                            "[c].[name] AS ColumnName, " +
                            "SCHEMA_NAME(ISNULL([st].[schema_id], [ut].[schema_id])) AS SchemaName, " +
                            "ISNULL([st].[name], [ut].[name]) AS DataTypeName " +
                        "FROM [sys].[table_types] tt " +
                            "INNER JOIN [sys].[columns] c " +
                                "ON	[c].[object_id] = [tt].[type_table_object_id] " +
                            "INNER JOIN [sys].[types] ut " +
                                "ON	[ut].[user_type_id] = [c].[user_type_id] " +
                            "LEFT OUTER JOIN [sys].[types] st " +
                                "ON	[st].[user_type_id] = [c].[system_type_id]" +
                        "WHERE " +
                            "[tt].[schema_id] = SCHEMA_ID(@SchemaName) " +
                            "AND [tt].[name] = @ObjectName;";

                    cmd.Parameters.AddWithValue("@stmt", stmt);
                    cmd.Parameters.AddWithValue("@params", "@SchemaName sysname, @ObjectName sysname");
                    cmd.Parameters.AddWithValue("@SchemaName", tableType.SchemaName);
                    cmd.Parameters.AddWithValue("@ObjectName", tableType.Name);


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var schemaName = reader.GetSqlString(1).Value;
                            var sqlTypeName = reader.GetSqlString(2).Value;
                            var clrTypeName = schemaName == "sys" ? Tools.ClrTypeName(sqlTypeName) : "System.Object";

                            retValue.Add(new TableTypeColumn(reader.GetSqlString(0).Value, clrTypeName));
                        }
                    }
                }
            }
            return retValue;
        }


        public IList<TableType> GetTableTypes()
        {
            var retValue = new List<TableType>();

            using (var conn = new SqlConnection(_designerConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_executesql";

                    const string stmt =
                        "SELECT SCHEMA_NAME([schema_id]) AS [SchemaName], [name] AS [ObjectName] FROM [sys].[table_types];";

                    cmd.Parameters.AddWithValue("@stmt", stmt);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            retValue.Add(new TableType(reader.GetSqlString(0).Value, reader.GetSqlString(1).Value));
                        }
                    }
                }
            }
            return retValue;
        }
    }
}
