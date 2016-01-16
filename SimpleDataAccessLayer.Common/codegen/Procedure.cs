using System;
using System.Collections.Generic;
using System.Linq;
using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.codegen
{
#if DEBUG 
    public class Procedure
#else 
    internal class Procedure
#endif
    {

        private readonly DalConfig _config;
        private readonly ISqlRepository _sqlRepository;
        private readonly bool _supportsAsync;

        public Procedure(DalConfig config, ISqlRepository sqlRepository, bool supportsAsync)
        {
            _config = config;
            if (sqlRepository == null)
                throw new ArgumentNullException(nameof(sqlRepository));
            _sqlRepository = sqlRepository;
            _supportsAsync = supportsAsync;
        }

        public string GetCode()
        {
            if (_config?.Procedures == null)
                return "";

            return string.Join("", _config.Procedures.Select(e => e.Schema)
                .Distinct().Select(ns =>
                    $"namespace {_config.Namespace}.Executables.{ns} {{{GetProceduresCodeForNamespace(ns)}}}"));
        }

        private string GetProceduresCodeForNamespace(string ns)
        {
            return string.Join("",
                _config.Procedures.Where(e => e.Schema == ns)
                    .Select(
                        proc =>
                            $"public class {Tools.ValidIdentifier(string.IsNullOrWhiteSpace(proc.Alias) ? proc.ProcedureName : proc.Alias)}{{{GetProcedureBodyCode(proc)}}}"));
        }

        private string GetProcedureBodyCode(config.models.Procedure proc)
        {
            var parameters = _sqlRepository.GetProcedureParameterCollection(proc.Schema, proc.ProcedureName);
            var recordsets = _sqlRepository.GetProcedureResultSetColumnCollection(proc.Schema, proc.ProcedureName);

            return
                $"{GetParameterCollectionCode(parameters)}public global::System.Int32 ReturnValue {{get; private set;}}{GetRecordsetsDefinitions(recordsets)}{GetExecuteCode(proc, parameters, recordsets)}";
        }

        private string GetExecuteCode(config.models.Procedure proc, IList<ProcedureParameter> parameters, IList<List<ProcedureResultSetColumn>> recordsets)
        {
            return string.Join("",
                new[] { _supportsAsync, false }.Distinct().Select(
                    async =>
                        $"public static {(async ? "async global::System.Threading.Tasks.Task<" : "")}{Tools.ValidIdentifier(proc.ProcedureName)}{(async ? ">" : "")} Execute{(async ? "Async" : "")} ({string.Join("", parameters.Select(parameter => $"global::{(parameter.IsTableType ? $"{_config.Namespace}.{parameter.ClrTypeName}" : parameter.ClrTypeName)} {parameter.AsLocalVariableName},"))} global::{_config.Namespace}.ExecutionScope executionScope = null, global::System.Int32 commandTimeout = 30){{{GetExecuteBodyCode(async, parameters, recordsets, proc)}}}/*end*/"));
        }

        private string GetExecuteBodyCode(bool async, IList<ProcedureParameter> parameters, IList<List<ProcedureResultSetColumn>> recordsets, config.models.Procedure proc)
        {
            var code =
                $"var retValue = new {Tools.ValidIdentifier(string.IsNullOrWhiteSpace(proc.Alias) ? proc.ProcedureName : proc.Alias)}();" +
                "{" +
                "   var retryCycle = 0;" +
                "   while (true) {" +
                $"       global::System.Data.SqlClient.SqlConnection conn = executionScope == null ? new global::System.Data.SqlClient.SqlConnection(global::{_config.Namespace}.ExecutionScope.ConnectionString) : executionScope.Transaction.Connection;" +
                "       try {" +
                "           if (conn.State != global::System.Data.ConnectionState.Open) {" +
                "               if (executionScope == null) {" +
                $"				{(async ? "await" : "")} conn.Open{(async ? "Async" : "")}();" +
                "			}" +
                "			else {" +
                "				retryCycle = int.MaxValue; " +
                "				throw new global::System.Exception(\"Execution Scope must have an open connection.\"); " +
                "			}" +
                "		}" +
                "	using (global::System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand()) {" +
                "			cmd.CommandType = global::System.Data.CommandType.StoredProcedure;" +
                "			if (executionScope != null && executionScope.Transaction != null)" +
                "				cmd.Transaction = executionScope.Transaction;" +
                "               cmd.CommandTimeout = commandTimeout;" +
                $"			cmd.CommandText = \"{Tools.QuoteName(proc.Schema)}.{Tools.QuoteName(proc.ProcedureName)}\";" +
                string.Join("", parameters.Select(p => p.IsTableType
                    ? $"cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter(\"@{p.ParameterName}\", global::System.Data.SqlDbType.Structured) {{TypeName = \"{p.SqlTypeName}\", Value = {p.AsLocalVariableName}.GetDataTable()}});"
                    : $"cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter(\"@{p.ParameterName}\", global::System.Data.SqlDbType.{Tools.SqlDbTypeName(p.SqlTypeName)}, {(("nchar nvarchar".Split(' ').Contains(p.SqlTypeName) && p.MaxByteLength != -1) ? (p.MaxByteLength/2) : p.MaxByteLength)}, global::System.Data.ParameterDirection.{(p.IsOutputParameter ? "Output" : "Input")}, true, {p.Precision}, {p.Scale}, null, global::System.Data.DataRowVersion.Default, {p.AsLocalVariableName}){{{("geography hierarchyid geometry".Split(' ').Contains(p.SqlTypeName) ? $"UdtTypeName = \"{p.SqlTypeName}\"" : "")}}});")) +
                "cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter(\"@ReturnValue\", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, true, 0, 0, null, global::System.Data.DataRowVersion.Default, global::System.DBNull.Value));" +
                (recordsets.Count == 0
                    ? $"{(async ? "await" : "")} cmd.ExecuteNonQuery{(async ? "Async" : "")}();"
                    : $"using (global::System.Data.SqlClient.SqlDataReader reader = {(async ? "await" : "")} cmd.ExecuteReader{(async ? "Async" : "")}()){{{MapRecordsetResults(async, recordsets)}}}") +
                $"retValue.Parameters = new ParametersCollection({string.Join(", ", parameters.Select(parameter => /* only copy from these the output parameters. Everything else shoud just be coming from the input params */ parameter.IsOutputParameter ? string.Format("cmd.Parameters[\"@{0}\"].Value == global::System.DBNull.Value ? null : (global::{1}) cmd.Parameters[\"@{0}\"].Value", parameter.ParameterName, $"{(parameter.IsTableType ? _config.Namespace + "." : "")}{parameter.ClrTypeName}") : parameter.AsLocalVariableName))});" +
                "retValue.ReturnValue = (global::System.Int32) cmd.Parameters[\"@ReturnValue\"].Value; " +
                "return retValue;" +
                "}" +
                "}" +
                "catch (global::System.Data.SqlClient.SqlException e) {" +
                "if (retryCycle++ > 9 || !ExecutionScope.RetryableErrors.Contains(e.Number))" +
                "   throw;" +
                "global::System.Threading.Thread.Sleep(1000);" +
                "}" +
                "finally {" +
                "if (executionScope == null &&  conn != null) {" +
                "((global::System.IDisposable) conn).Dispose();" +
                "}" +
                "}" +
                "}" +
                //                "}" +
                "}"
                ;

            return code;

        }

        private string MapRecordsetResults(bool @async, IList<List<ProcedureResultSetColumn>> recordsets)
        {
            var code = "";
            for (var rsNo = 0; rsNo < recordsets.Count; rsNo++)
            {
                var recordset = recordsets[rsNo];
                var internalCode = string.Format("retValue.Recordset{0} = new global::System.Collections.Generic.List<Record{0}>(); while ({1} reader.Read{2}()) {{{3}}}",
                    rsNo,
                    async ? "await" : "",
                    async ? "Async" : "",
                    string.Format("retValue.Recordset{0}.Add(new Record{0}({1}));",
                        rsNo,
                        MapRecord(async, recordset))
                    );
                if (rsNo > 0)
                {
                    internalCode =
                        $"if ({(async ? "await" : "")} reader.NextResult{(async ? "Async" : "")}()){{{internalCode}}}";
                }

                code += internalCode;
            }


            return code;
        }

        private string MapRecord(bool async, List<ProcedureResultSetColumn> recordset)
        {
            var code = "";

            for (var colNo = 0; colNo < recordset.Count; colNo++)
            {
                var column = recordset[colNo];
                code += string.Format("{4} reader.IsDBNull({0}) ? null : {1} reader.GetFieldValue{2}<global::{3}>({0})",
                    colNo,
                    async ? "await" : "",
                    async ? "Async" : "",
                    column.ClrTypeName,
                    colNo > 0 ? "," : ""
                    );
            }

            return code;
        }

        private string GetRecordsetsDefinitions(IList<List<ProcedureResultSetColumn>> recordsets)
        {
            var code = "";

            for (var rsNo = 0; rsNo < recordsets.Count; rsNo++)
            {
                var recordset = recordsets[rsNo];

                code += string.Format("public class Record{0} {{{1}public Record{0}({2}){{{3}}}}}public global::System.Collections.Generic.List<Record{0}> Recordset{0}{{get;private set;}}",
                    rsNo,
                    string.Join("",
                        recordset.Select(
                            column =>
                                $"public global::{column.ClrTypeName} {Tools.ValidIdentifier(column.ColumnName)} {{get; private set;}}")),
                    string.Join(",",
                        recordset.Select(
                            column =>
                                $"global::{column.ClrTypeName} {Tools.ValidIdentifier(column.ColumnName).Substring(0, 1).ToLowerInvariant() + Tools.ValidIdentifier(column.ColumnName).Substring(1)}")),
                    string.Join("",
                        recordset.Select(
                            column =>
                                $"this.{column.ColumnName} = {Tools.ValidIdentifier(column.ColumnName).Substring(0, 1).ToLowerInvariant() + Tools.ValidIdentifier(column.ColumnName).Substring(1)};"))
                    );
            }

            return code;
        }

        private string GetParameterCollectionCode(IList<ProcedureParameter> parameters)
        {
            return
                $"public class ParametersCollection {{{string.Join("", parameters.Select(p => $"public global::{string.Format($"{(p.IsTableType ? _config.Namespace + "." : "")}{p.ClrTypeName}", p.IsTableType ? _config.Namespace + "." : "", p.ClrTypeName)} {p.ParameterName} {{get;private set;}}"))}public ParametersCollection({string.Join(",", parameters.Select(p => $"global::{string.Format($"{(p.IsTableType ? _config.Namespace + "." : "")}{p.ClrTypeName}", p.IsTableType ? _config.Namespace + "." : "", p.ClrTypeName)} {p.AsLocalVariableName}"))}){{{string.Join("", parameters.Select(p => $"this.{p.ParameterName} = {p.AsLocalVariableName};"))}}}}}public ParametersCollection Parameters {{get;private set;}}";
        }
    }
}
