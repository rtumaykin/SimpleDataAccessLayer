using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using SimpleDataAccessLayer.Common.config.models;
using Enum = SimpleDataAccessLayer.Common.config.models.Enum;

namespace SimpleDataAccessLayer.Common.wizard
{
    public partial class EnumsTab : UserControl, IUseDalConfig, IValidateInput, IRefreshTabOnActivate
    {
        // I will keep a copy of it here to be able to create connection strings
        private DalConfig _config;
        private bool _isLoading;
        private bool _isRowUpdating;
        private readonly Dictionary<string, Enum> _configEnumsCollection = new Dictionary<string, Enum>();
        public EnumsTab()
        {
            InitializeComponent();
        }

        public void UpdateDalConfig(DalConfig config)
        {
            config.Enums = (from DataGridViewRow row in enumsGrid.Rows
                            where (bool)row.Cells["GenerateInterface"].Value
                            select new Enum()
                            {
                                Schema = (string)row.Cells["Schema"].Value,
                                TableName = (string)row.Cells["TableName"].Value,
                                KeyColumn = (string)row.Cells["KeyColumn"].Value,
                                ValueColumn = (string)row.Cells["ValueColumn"].Value,
                                Alias = (string)row.Cells["Alias"].Value
                            }).ToList();
        }

        public void InitializeFromDalConfig(DalConfig config)
        {
            _config = config;
            if (_config?.Enums == null)
                return;

            foreach (var _enum in _config.Enums)
            {
                _configEnumsCollection.Add(Utilities.QuoteName(_enum.Schema) + "." + Utilities.QuoteName(_enum.TableName), _enum);
            }
        }

        public string ValidateInput()
        {
            return "";
        }

        private static void SetDefaultsForDropDownCells(DataGridViewRow row)
        {
            if (string.IsNullOrWhiteSpace((string)(row.Cells["KeyColumn"].Value)))
                row.Cells["KeyColumn"].Value = ((DataGridViewComboBoxCell)row.Cells["KeyColumn"]).Items[0];

            if (string.IsNullOrWhiteSpace((string)(row.Cells["ValueColumn"].Value)))
                row.Cells["ValueColumn"].Value = ((DataGridViewComboBoxCell)row.Cells["ValueColumn"]).Items[0];
        }

        private void RepopulateEnumsGrid()
        {
            const string query = @"
				SELECT	[SchemaName],
						[TableName],
						CONVERT(xml, [ValueColumnsXml]) AS [ValueColumnsXml],
						CONVERT(xml, [KeyColumnsXml]) AS [KeyColumnsXml]
				FROM (
					SELECT 
						OBJECT_SCHEMA_NAME([object_id]) AS SchemaName,
						OBJECT_NAME([object_id]) AS TableName, 
						[object_id], 
					(
						SELECT [column_id] AS Value, [name] AS [Key]
						FROM [sys].[columns] ValueColumns
						WHERE 
							[object_id] = o.[object_id]
							AND [system_type_id] IN (48, 52, 56, 104, 127)
						FOR XML AUTO, ROOT('data')
					) AS ValueColumnsXml,
					(
						SELECT [column_id] AS Value, [name] AS [Key]
						FROM [sys].[columns] KeyColumns
						WHERE 
							[object_id] = o.[object_id]
							AND [system_type_id] IN (167, 175, 231, 239)
						FOR XML AUTO, ROOT('data')
					) AS KeyColumnsXml
					FROM [sys].[objects] o
					WHERE [type] IN ('U', 'V')
				) x
				WHERE [ValueColumnsXml] IS NOT NULL AND [KeyColumnsXml] IS NOT NULL
				ORDER BY 
					[SchemaName] ASC,
					[TableName] ASC;";

            using (var conn = new SqlConnection(Utilities.BuildConnectionString(_config)))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sys.sp_executesql";
                    cmd.Parameters.AddWithValue("@stmt", query);

                    using (var reader = cmd.ExecuteReader())
                    {
                        // since this happens only when connection server and database changes, I can wipe old items
                        enumsGrid.Rows.Clear();

                        _isLoading = true;
                        try
                        {
                            while (reader.Read())
                            {
                                AddRow(reader.GetFieldValue<string>(0), reader.GetFieldValue<string>(1), reader.GetFieldValue<string>(2), reader.GetFieldValue<string>(3));
                            }
                        }
                        finally
                        {
                            _isLoading = false;
                        }
                    }
                }
            }
        }

        private void AddRow(string schemaName, string tableName, string valueColumnsXml, string keyColumnsXml)
        {
            // Create new row and get the cell templates
            var row = enumsGrid.Rows[enumsGrid.Rows.Add()];
            var tableSchemaCell = (DataGridViewTextBoxCell)row.Cells["Schema"];
            var tableNameCell = (DataGridViewTextBoxCell)row.Cells["TableName"];
            var keysCell = (DataGridViewComboBoxCell)row.Cells["KeyColumn"];
            var valuesCell = (DataGridViewComboBoxCell)row.Cells["ValueColumn"];
            var alias = (DataGridViewTextBoxCell)row.Cells["Alias"];
            var generate = (DataGridViewCheckBoxCell)row.Cells["GenerateInterface"];

            tableSchemaCell.Value = schemaName;
            tableNameCell.Value = tableName;
            var quotedName = Utilities.QuoteName(schemaName) + "." + Utilities.QuoteName(tableName);
            var isEnumInConfig = _configEnumsCollection.ContainsKey(quotedName);
            alias.Value = isEnumInConfig ? _configEnumsCollection[quotedName].Alias : "";
            generate.Value = isEnumInConfig;

            var keysDataSet = new DataSet();
            keysDataSet.ReadXml(new StringReader(keyColumnsXml));

            foreach (var key in (from DataRow dataRow in keysDataSet.Tables["KeyColumns"].Rows select (string)dataRow["Key"]).Where(key => keysCell != null))
            {
                keysCell.Items.Add(key);
            }

            var valuesDataSet = new DataSet();
            valuesDataSet.ReadXml(new StringReader(valueColumnsXml));

            foreach (DataRow dataRow in valuesDataSet.Tables["ValueColumns"].Rows)
            {
                valuesCell.Items.Add((string)dataRow["Key"]);
            }

            if (isEnumInConfig)
            {
                if (keysCell != null) keysCell.Value = _configEnumsCollection[quotedName].KeyColumn;
                valuesCell.Value = _configEnumsCollection[quotedName].ValueColumn;
            }
        }


        public void RefreshTabOnActivate()
        {
            RepopulateEnumsGrid();
            var keysToRemove =
                _configEnumsCollection.Where(
                    enumToEvaluate =>
                        enumsGrid.Rows.Cast<DataGridViewRow>()
                            .All(
                                row =>
                                    Utilities.QuoteName($"{row.Cells["Schema"].Value}") + "." +
                                    Utilities.QuoteName($"{row.Cells["TableName"].Value}") != enumToEvaluate.Key))
                    .Select(enumToEvaluate => enumToEvaluate.Key)
                    .ToList();

            foreach (var key in keysToRemove)
            {
                _configEnumsCollection.Remove(key);
            }
        }

        private void CellValueChangedHandler(object sender, DataGridViewCellEventArgs e)
        {
            if (DesignMode) 
                return;

            if (e.RowIndex < 0)
                return;

            // this is happening within the same thread
            if (_isLoading)
                return;

            // This row is already updating 
            if (_isRowUpdating)
                return;

            // if none of the above let's start row updating;
            _isRowUpdating = true;
            try
            {
                var row = enumsGrid.Rows[e.RowIndex];
                var tableName = Utilities.QuoteName($"{row.Cells["Schema"].Value}") + "." +
                                Utilities.QuoteName($"{row.Cells["TableName"].Value}");

                // this was a check box
                if (enumsGrid.Columns[e.ColumnIndex].Name == "GenerateInterface")
                {
                    // if it was set to true, then need to make sure all columns are selected
                    if ((bool) ((DataGridViewCheckBoxCell) (row.Cells[e.ColumnIndex])).Value)
                    {
                        SetDefaultsForDropDownCells(row);
                        _configEnumsCollection.Add(
                            tableName, new Enum()
                            {
                                Schema = (string) row.Cells["Schema"].Value,
                                TableName = (string) row.Cells["TableName"].Value,
                                KeyColumn = (string) row.Cells["KeyColumn"].Value,
                                ValueColumn = (string) row.Cells["ValueColumn"].Value,
                                Alias = (string) row.Cells["Alias"].Value
                            });
                    }
                    else
                    {
                        // remove all data from the row
                        row.Cells["KeyColumn"].Value = row.Cells["ValueColumn"].Value = row.Cells["Alias"].Value = "";
                        _configEnumsCollection.Remove(tableName);
                    }
                }
                else
                {
                    // Generate is not checked - add row
                    if (((DataGridViewCheckBoxCell) (row.Cells["GenerateInterface"])).Value != null &&
                        !(bool) ((DataGridViewCheckBoxCell) (row.Cells["GenerateInterface"])).Value)
                    {
                        SetDefaultsForDropDownCells(row);

                        ((DataGridViewCheckBoxCell) (row.Cells["GenerateInterface"])).Value = true;
                        _configEnumsCollection.Add(
                            tableName, new Enum()
                            {
                                Schema = (string) row.Cells["Schema"].Value,
                                TableName = (string) row.Cells["TableName"].Value,
                                KeyColumn = (string) row.Cells["KeyColumn"].Value,
                                ValueColumn = (string) row.Cells["ValueColumn"].Value,
                                Alias = (string) row.Cells["Alias"].Value
                            });
                    }
                    else if ((bool)((DataGridViewCheckBoxCell)(row.Cells["GenerateInterface"])).Value)
                    {
                        if (_configEnumsCollection.ContainsKey(tableName))
                        {
                            var enumToUpdate = _configEnumsCollection[tableName];

                            enumToUpdate.Alias = (string) row.Cells["Alias"].Value;
                            enumToUpdate.KeyColumn = (string) row.Cells["KeyColumn"].Value;
                            enumToUpdate.ValueColumn = (string) row.Cells["ValueColumn"].Value;
                        }
                        else
                        {
                            _configEnumsCollection.Add(
                                tableName, new Enum()
                                {
                                    Schema = (string) row.Cells["Schema"].Value,
                                    TableName = (string) row.Cells["TableName"].Value,
                                    KeyColumn = (string) row.Cells["KeyColumn"].Value,
                                    ValueColumn = (string) row.Cells["ValueColumn"].Value,
                                    Alias = (string) row.Cells["Alias"].Value
                                });
                        }
                    }
                }
            }
            finally
            {
                _isRowUpdating = false;
            }

        }

        private void ForceEndEditMode(object sender, EventArgs e)
        {
            if (enumsGrid.IsCurrentCellDirty)
            {
                enumsGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
    }
}
