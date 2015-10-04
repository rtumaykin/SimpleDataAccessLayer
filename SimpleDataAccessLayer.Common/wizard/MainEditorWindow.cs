using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.wizard
{
    public partial class MainEditorWindow : UserControl
    {
        private DalConfig _config;

        public DalConfig Config => _config;

        public MainEditorWindow()
        {
            InitializeComponent();
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            var tempConfig = _config.Clone();
            if (new ModelWizard(tempConfig).ShowDialog(this) == DialogResult.OK)
            {
                _config = tempConfig.Clone();
                RefreshEditor();
            }
        }

        private void RefreshEditor()
        {
            ConnectionStringName.Text = _config.RuntimeConnectionStringName;
            Server.Text = _config.DesignerConnection.ServerName;
            Database.Text = _config.DesignerConnection.DatabaseName;
            Namespace.Text = _config.Namespace;

            EnumsGrid.Rows.Clear();
            foreach (var _enum in Config.Enums)
            {
                EnumsGrid.Rows.Add(_enum.Schema, _enum.TableName, _enum.Alias, _enum.KeyColumn, _enum.ValueColumn);
            }

            ProcsGrid.Rows.Clear();
            foreach (var procedure in Config.Procedures)
            {
                ProcsGrid.Rows.Add(procedure.Schema, procedure.ProcedureName, procedure.Alias);
            }
        }

        public void Init(string configSerialized)
        {
            try
            {
                _config = JsonConvert.DeserializeObject<DalConfig>(configSerialized, new StringEnumConverter());
            }
            catch (Exception je)
            {
                try
                {
                    // if the json deserialization did not work try xml
                    _config = SimpleDataAccessLayer.Common.utilities.DalConfigXmlConverter.ParseDalConfigXml(configSerialized);
                }
                catch (Exception xe)
                {
                    MessageBox.Show(
                        $"Failed to deserealize config file.\r\nJSON error: \"{je.Message}\"\r\nXML error: \"{xe.Message}\"\r\nBlank config will be used.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _config = new DalConfig();
                }
            }

            RefreshEditor();
        }
    }
}
