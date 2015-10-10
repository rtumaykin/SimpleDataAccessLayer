using System;
using System.Collections.Generic;
using System.Linq;
using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.codegen
{

#if DEBUG 
    public class TableValuedParameter
#else 
    internal class TableValuedParameter
#endif
    {
        private readonly DalConfig _config;
        private readonly ISqlRepository _sqlRepository;

        public TableValuedParameter(DalConfig config, ISqlRepository sqlRepository)
        {
            _config = config;
            if (sqlRepository == null)
                throw new ArgumentNullException(nameof(sqlRepository));
            _sqlRepository = sqlRepository;
        }

        public string GetCode()
        {
            var tableTypes = _sqlRepository.GetTableTypes();

            if (tableTypes.Count == 0)
                return "";

            // ReSharper disable once UseStringInterpolation -- it is cleaner this way
            return string.Join("", tableTypes.Select(tt => tt.SchemaName).Distinct().Select(schema => string.Format("namespace {0}.{1}.{2} {{{3}}}",
                _config.Namespace,
                "TableVariables",
                Tools.ValidIdentifier(schema),
                string.Join("",
                    tableTypes.Where(ttn => ttn.SchemaName == schema)
                        .Select(ttn =>
                        {
                            var columns = _sqlRepository.GetTableTypeColumns(ttn);
                            return string.Format(
                                "public class {0}Row {{{1}}}" +
                                "public class {0} : global::System.Collections.Generic.List<{0}Row> {{public {0} (global::System.Collections.Generic.IEnumerable<{0}Row> collection) : base(collection){{}}internal global::System.Data.DataTable GetDataTable() {{{2}}}}}",
                                Tools.ValidIdentifier(ttn.Name),
                                GetCodeForTableTypeColumns(ttn, columns),
                                GetCodeForDataTableConversion(columns));
                        }))
                )));
        }

        private string GetCodeForDataTableConversion(IList<TableTypeColumn> columns)
        {
            var code = "var dt = new global::System.Data.DataTable();\r";
            code += string.Join("\r",
                columns.Select(
                    column => $"dt.Columns.Add(\"{column.ColumnName}\", typeof({column.ClrTypeName.Replace("?", "")}));"));

            code +=
                "dt.AcceptChanges();\r" +
                "foreach (var item in this) {" +
                "var row = dt.NewRow();";
            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                code += $"row[{i}] = item.{column.ColumnName};\r";
            }
            code += "dt.Rows.Add(row);\r";

            code += "}\rreturn dt;";
            return code;
        }

        private string GetCodeForTableTypeColumns(TableType tableType, IList<TableTypeColumn> columns)
        {
            var code = string.Join("\r", columns.Select(
                column =>
                    $"public global::{column.ClrTypeName} {Tools.ValidIdentifier(column.ColumnName)} {{ get; private set; }}\r"));

            code +=
                $"public {Tools.ValidIdentifier(tableType.Name)}Row({string.Join(", ", columns.Select(column => $"global::{column.ClrTypeName} {column.ColumnName.Substring(0, 1).ToLower() + column.ColumnName.Substring(1)}"))}) {{{string.Join("\r", columns.Select(column => $"this.{column.ColumnName} = {column.ColumnName.Substring(0, 1).ToLower() + column.ColumnName.Substring(1)};"))}}}\r";
            return code;
        }
    }
}
