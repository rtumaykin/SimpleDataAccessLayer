using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.wizard
{
    public partial class DesignerConnectionTab : UserControl, IUseDalConfig, IValidateInput
    {
        private string _sqlUserName = "", _sqlPassword = "";
        private bool _revalidationNeeded = true;
        private bool _sqlSavePassword;

        public DesignerConnectionTab()
        {
            InitializeComponent();
        }

        // consistency validation will happen when the user hits next button.

        private void Authentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            MarkDatabaseListForRevalidation();

            if (Authentication.SelectedIndex == 0)
            {

                // save the sql username and password in case user reverts back
                _sqlPassword = Password.Text;
                _sqlUserName = UserName.Text;
                _sqlSavePassword = SavePassword.Checked;

                // clear password, disable it, put Windows user into the UserName and also disable it
                Password.Text = "";
                Password.Enabled = false;

                UserName.Text = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;
                UserName.Enabled = false;

                SavePassword.Checked = false;
                SavePassword.Enabled = false;
            }
            else
            {
                // restore the sql username and password
                Password.Text = _sqlPassword;
                UserName.Text = _sqlUserName;

                // enable UserName and Password
                Password.Enabled = true;
                UserName.Enabled = true;

                SavePassword.Checked = _sqlSavePassword;
                SavePassword.Enabled = true;
            }
        }

        private void MarkDatabaseListForRevalidation()
        {
            _revalidationNeeded = true;
        }

        public void UpdateDalConfig(DalConfig config)
        {
            var validationResult = ValidateInput();
            if (!string.IsNullOrWhiteSpace(validationResult))
            {
                throw new Exception(validationResult);
            }

            config.DesignerConnection = new DesignerConnection()
            {
                Authentication = Authentication.SelectedIndex == 0
                    ? new Authentication {AuthenticationType = AuthenticationType.WindowsAuthentication}
                    : (Authentication) new Authentication
                    {
                        AuthenticationType = AuthenticationType.SqlAuthentication,
                        UserName = UserName.Text,
                        Password = Password.Text,
                        SavePassword = SavePassword.Checked
                    },
                ServerName = ServerName.Text,
                DatabaseName = (string) DatabaseName.SelectedItem
            };

            config.Namespace = Namespace.Text;
            config.RuntimeConnectionStringName = ConnectionString.Text;
        }

        private void DatabaseName_ReloadItems(object sender, EventArgs e)
        {
            if (!_revalidationNeeded)
                return;
            try
            {
                var connString = BuildConnectionString();

                using (var con = new SqlConnection(connString))
                {
                    con.Open();
                    var databases = con.GetSchema("Databases");
                    var nameColumnId = databases.Columns["database_name"].Ordinal;
                    var currentDatabaseName = (string) DatabaseName.SelectedItem;
                    DatabaseName.Items.Clear();
                    foreach (var index in from DataRow database in databases.Rows
                        select (string) database.ItemArray[nameColumnId]
                        into databaseName
                        let index = DatabaseName.Items.Add(databaseName)
                        where databaseName == currentDatabaseName
                        select index)
                    {
                        DatabaseName.SelectedIndex = index;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string BuildConnectionString()
        {
            var ssb = new SqlConnectionStringBuilder {DataSource = ServerName.Text};

            if (Authentication.SelectedIndex == 0)
            {
                ssb.IntegratedSecurity = true;
            }
            else
            {
                ssb.UserID = UserName.Text;
                ssb.Password = Password.Text;
            }

            return ssb.ConnectionString;
        }

        public void InitializeFromDalConfig(DalConfig config)
        {
            ServerName.Text = config?.DesignerConnection?.ServerName;

            if (config?.DesignerConnection?.Authentication?.AuthenticationType ==
                AuthenticationType.SqlAuthentication)
            {
                _sqlPassword = config?.DesignerConnection?.Authentication?.Password;
                _sqlUserName = config?.DesignerConnection?.Authentication?.UserName;
                var savePasswordNullable = config?.DesignerConnection?.Authentication?.SavePassword;
                _sqlSavePassword = savePasswordNullable.HasValue && savePasswordNullable.Value;
            }

            Authentication.SelectedIndex = config?.DesignerConnection?.Authentication?.AuthenticationType ==
                                           AuthenticationType.SqlAuthentication ? 1 : 0;

            ServerName.Text = config?.DesignerConnection?.ServerName;
            if (!string.IsNullOrWhiteSpace(config?.DesignerConnection?.DatabaseName))
            {
                DatabaseName.Items.Add(config?.DesignerConnection?.DatabaseName);
                DatabaseName.SelectedIndex = 0;
            }

            Namespace.Text = config?.Namespace;
            ConnectionString.Text = config?.RuntimeConnectionStringName;
        }

        private void ServerName_TextChanged(object sender, EventArgs e)
        {
            MarkDatabaseListForRevalidation();
        }

        private void UserName_TextChanged(object sender, EventArgs e)
        {
            MarkDatabaseListForRevalidation();
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            MarkDatabaseListForRevalidation();
        }

        public string ValidateInput()
        {
            var errors = "";

            if (string.IsNullOrWhiteSpace(ServerName.Text))
                errors += "Server Name can not be empty\r\n";

            if (string.IsNullOrWhiteSpace(Namespace.Text))
                errors += "Namespace can not be empty\r\n";

            if (string.IsNullOrWhiteSpace(ConnectionString.Text))
                errors += "Runtime connection string can not be empty\r\n";

            if (DatabaseName.SelectedIndex < 0)
                errors += "Database name can not be empty\r\n";


            if (!_revalidationNeeded)
                return errors;
            try
            {
                var connString = BuildConnectionString();

                using (var con = new SqlConnection(connString))
                {
                    con.Open();
                    var databases = con.GetSchema("Databases");
                    var nameColumnId = databases.Columns["database_name"].Ordinal;

                    if (
                        databases.Rows.Cast<DataRow>()
                            .Any(
                                database =>
                                    (string) database.ItemArray[nameColumnId] ==
                                    (string) DatabaseName.SelectedItem))
                    {
                        return errors;
                    }
                }

                return errors + $"The database \"{(string) DatabaseName.SelectedItem}\" does not exist on server \"{ServerName.Text}\"";
            }
            catch (Exception e)
            {
                return errors + e.Message;
            }
        }
    }
}
