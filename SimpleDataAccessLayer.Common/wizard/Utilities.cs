using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.wizard
{
    static class Utilities
    {
        internal static string QuoteName(string name)
        {
            if (name == null)
                return null;

            return ("[" + name.Replace("]", "]]") + "]");
        }

        internal static string BuildConnectionString(DalConfig config)
        {
            var csb = new SqlConnectionStringBuilder
            {
                DataSource = config.DesignerConnection.ServerName,
                InitialCatalog = config.DesignerConnection.DatabaseName
            };

            if (config.DesignerConnection.Authentication.AuthenticationType == AuthenticationType.SqlAuthentication)
            {
                csb.UserID = config.DesignerConnection.Authentication.UserName;
                csb.Password = config.DesignerConnection.Authentication.Password;
            }
            else
            {
                csb.IntegratedSecurity = true;
            }

            return csb.ConnectionString;
        }
    }
}
