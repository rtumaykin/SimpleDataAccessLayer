using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.wizard
{
    public partial class ProceduresTab : UserControl, IRefreshTabOnActivate, IUseDalConfig, IValidateInput
    {
        private DalConfig _config;
        private bool _isLoading;
        private bool _isRowUpdating;
        private readonly Dictionary<string, Procedure> _configProceduresCollection = new Dictionary<string, Procedure>();
        public ProceduresTab()
        {
            InitializeComponent();
        }

        public void RefreshTabOnActivate()
        {
            RepopulateProceduresGrid();
            var keysToRemove =
                _configProceduresCollection.Where(
                    procedureToEvaluate =>
                        proceduresGrid.Rows.Cast<DataGridViewRow>()
                            .All(
                                row =>
                                    Utilities.QuoteName($"{row.Cells["Schema"].Value}") + "." +
                                    Utilities.QuoteName($"{row.Cells["ProcedureName"].Value}") !=
                                    procedureToEvaluate.Key))
                    .Select(procedureToEvaluate => procedureToEvaluate.Key)
                    .ToList();

            foreach (var key in keysToRemove)
            {
                _configProceduresCollection.Remove(key);
            }
        }

        public void UpdateDalConfig(DalConfig config)
        {
            config.Procedures = (from DataGridViewRow row in proceduresGrid.Rows
                                 where (bool)row.Cells["GenerateInterface"].Value
                                 select new Procedure()
                                 {
                                     Schema = (string)row.Cells["Schema"].Value,
                                     ProcedureName = (string)row.Cells["ProcedureName"].Value,
                                     Alias = (string)row.Cells["Alias"].Value
                                 }).ToList();
        }

        public void InitializeFromDalConfig(DalConfig config)
        {
            _config = config;
            if (_config?.Procedures == null)
                return;

            foreach (var procedure in _config.Procedures)
            {
                _configProceduresCollection.Add(Utilities.QuoteName(procedure.Schema) + "." + Utilities.QuoteName(procedure.ProcedureName), procedure);
            }
        }

        public string ValidateInput()
        {
            return "";
        }

        private void ForceEndEditMode(object sender, EventArgs e)
        {
            if (proceduresGrid.IsCurrentCellDirty)
            {
                proceduresGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
        void CellValueChangedHandler(object sender, DataGridViewCellEventArgs e)
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
                var row = proceduresGrid.Rows[e.RowIndex];
                var procedureName = Utilities.QuoteName($"{row.Cells["Schema"].Value}") + "." +
                                Utilities.QuoteName($"{row.Cells["ProcedureName"].Value}");

                // this was a check box
                if (proceduresGrid.Columns[e.ColumnIndex].Name == "GenerateInterface")
                {
                    // if it was set to true, then need to make sure all columns are selected
                    if (!((bool) ((DataGridViewCheckBoxCell) (row.Cells[e.ColumnIndex])).Value))
                    {
                        // remove all data from the row
                        row.Cells["Alias"].Value = "";
                        _configProceduresCollection.Remove(procedureName);
                    }
                    else
                    {
                        _configProceduresCollection.Add(procedureName, new Procedure()
                        {
                            Schema = (string) row.Cells["Schema"].Value,
                            ProcedureName = (string) row.Cells["ProcedureName"].Value,
                            Alias = (string) row.Cells["Alias"].Value
                        });
                    }
                }
                else
                {
                    // Generate is already checked - do nothing
                    if (((DataGridViewCheckBoxCell) (row.Cells["GenerateInterface"])).Value != null &&
                        !(bool) ((DataGridViewCheckBoxCell) (row.Cells["GenerateInterface"])).Value)
                    {
                        ((DataGridViewCheckBoxCell) (row.Cells["GenerateInterface"])).Value = true;
                        _configProceduresCollection.Add(procedureName, new Procedure()
                        {
                            Schema = (string) row.Cells["Schema"].Value,
                            ProcedureName = (string) row.Cells["ProcedureName"].Value,
                            Alias = (string) row.Cells["Alias"].Value
                        });
                    }
                    else
                    {
                        if (_configProceduresCollection.ContainsKey(procedureName))
                        {
                            var procToUpdate = _configProceduresCollection[procedureName];
                            procToUpdate.Alias = (string) row.Cells["Alias"].Value;
                        }
                        else
                        {
                            _configProceduresCollection.Add(procedureName, new Procedure()
                            {
                                Schema = (string)row.Cells["Schema"].Value,
                                ProcedureName = (string)row.Cells["ProcedureName"].Value,
                                Alias = (string)row.Cells["Alias"].Value
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

        private void RepopulateProceduresGrid()
        {
            const string query = @"
				SELECT 
					OBJECT_SCHEMA_NAME([object_id]) AS SchemaName, 
					OBJECT_NAME([object_id]) AS ProcedureName
				FROM [sys].[objects] o
				WHERE [type] = 'P'
				ORDER BY 
					OBJECT_SCHEMA_NAME([object_id]) ASC, 
					OBJECT_NAME([object_id]) ASC;";

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
                        proceduresGrid.Rows.Clear();

                        _isLoading = true;
                        try
                        {
                            while (reader.Read())
                            {
                                AddRow(reader.GetFieldValue<string>(0), reader.GetFieldValue<string>(1));
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

        private void AddRow(string procedureSchema, string procedureName)
        {
            // Create new row and get the cell templates
            var row = proceduresGrid.Rows[proceduresGrid.Rows.Add()];

            var procedureSchemaCell = (DataGridViewTextBoxCell)row.Cells["Schema"];
            var procedureNameCell = (DataGridViewTextBoxCell)row.Cells["ProcedureName"];
            var alias = (DataGridViewTextBoxCell)row.Cells["Alias"];
            var generate = (DataGridViewCheckBoxCell)row.Cells["GenerateInterface"];

            var quotedName = Utilities.QuoteName(procedureSchema) + "." + Utilities.QuoteName(procedureName);

            procedureSchemaCell.Value = procedureSchema;
            procedureNameCell.Value = procedureName;
            var isEnumInConfig = _configProceduresCollection.ContainsKey(quotedName);
            alias.Value = isEnumInConfig ? _configProceduresCollection[quotedName].Alias : "";
            generate.Value = isEnumInConfig;
        }
    }
}
