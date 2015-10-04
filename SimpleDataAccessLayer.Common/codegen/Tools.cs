using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleDataAccessLayer.Common.codegen
{
    public static class Tools
    {
        public static string QuoteName(string name)
        {
            return ("[" + name.Replace("]", "]]") + "]");
        }

        public static string ClrTypeName(string sqlTypeName)
        {
            var typeMapping = new Dictionary<string, string>
            {
                {"bigint", "System.Int64?"},
                {"binary", "System.Byte[]"},
                {"bit", "System.Boolean?"},
                {"char", "System.String"},
                {"date", "System.DateTime?"},
                {"datetime", "System.DateTime?"},
                {"datetime2", "System.DateTime?"},
                {"datetimeoffset", "System.DateTimeOffset?"},
                {"decimal", "System.Decimal?"},
                {"float", "System.Double?"},
                {"geography", "Microsoft.SqlServer.Types.SqlGeography"},
                {"geometry", "Microsoft.SqlServer.Types.SqlGeometry"},
                {"hierarchyid", "Microsoft.SqlServer.Types.SqlHierarchyId?"},

                {"int", "System.Int32?"},
                {"money", "System.Decimal?"},
                {"nchar", "System.String"},

                {"numeric", "System.Decimal?"},
                {"nvarchar", "System.String"},
                {"real", "System.Single?"},
                {"smalldatetime", "System.DateTime?"},
                {"smallint", "System.Int16?"},
                {"smallmoney", "System.Decimal?"},
                {"sql_variant", "System.Object"},

                {"time", "System.TimeSpan?"},
                {"timestamp", "System.Byte[]"},
                {"tinyint", "System.Byte?"},
                {"uniqueidentifier", "System.Guid?"},
                {"varbinary", "System.Byte[]"},
                {"varchar", "System.String"},
                {"xml", "System.String"}
            };

            string clrTypeName = null;

            if (typeMapping.ContainsKey(sqlTypeName))
            {
                clrTypeName = typeMapping[sqlTypeName];
            }
            return clrTypeName;
        }

        public static string ValidIdentifier(string name)
        {
            var rgx = new Regex("[^a-zA-Z0-9_]");
            return rgx.Replace(name, "_");
        }

        public static string SqlDbTypeName(string sqlTypeName, bool isTableType = false)
        {
            var sqlDbTypeMapping = new Dictionary<string, string>()
            {
                {"bigint", "BigInt"},
                {"binary", "Binary"},
                {"bit", "Bit"},
                {"char", "Char"},
                {"date", "Date"},
                {"datetime", "DateTime"},
                {"datetime2", "DateTime2"},
                {"datetimeoffset", "DateTimeOffset"},
                {"decimal", "Decimal"},
                {"float", "Float"},
                {"int", "Int"},
                {"money", "Money"},
                {"nchar", "NChar"},
                {"nvarchar", "NVarChar"},
                {"numeric", "Decimal"},
                {"real", "Real"},
                {"smalldatetime", "SmallDateTime"},
                {"smallint", "SmallInt"},
                {"smallmoney", "SmallMoney"},
                {"time", "Time"},
                {"timestamp", "Timestamp"},
                {"tinyint", "TinyInt"},
                {"geometry", "Udt"},
                {"geography", "Udt"},
                {"hierarchyid", "Udt"},
                {"uniqueidentifier", "UniqueIdentifier"},
                {"varbinary", "VarBinary"},
                {"varchar", "VarChar"},
                {"sql_variant", "Variant"},
                {"xml", "Xml"}
            };
            return sqlDbTypeMapping.ContainsKey(sqlTypeName)
                ? sqlDbTypeMapping[sqlTypeName]
                : (isTableType ? "Structured" : null);
        }
    }
}

