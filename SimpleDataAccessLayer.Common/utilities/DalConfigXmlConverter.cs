using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.utilities
{
    public class DalConfigXmlConverterXmlParsingException : Exception
    {
        public DalConfigXmlConverterXmlParsingException(Exception e) : base("Failed to parse DalConfig Xml", e)
        {
        }
    }

    public class DalConfigXmlConverterException : Exception
    {
        public DalConfigXmlConverterException(string message, Exception e) : base(message, e)
        {
        }
    }

    public static class DalConfigXmlConverter
    {
        public static DalConfig ParseDalConfigXml(string dalConfigXml)
        {
            try
            {
                var ser = new DataContractSerializer(typeof (config.models.old.DalConfig));
                var config =
                    (config.models.old.DalConfig) ser.ReadObject(XmlReader.Create(new StringReader(dalConfigXml)));

                // now try to read and parse the the connection string
                var connString =
                    ConfigurationManager.ConnectionStrings[config.ApplicationConnectionString]?.ConnectionString;

                var csb = new SqlConnectionStringBuilder(connString);


                return new DalConfig()
                {
                    DesignerConnection = new DesignerConnection()
                    {
                        Authentication =
                            ((config.DesignerConnection?.Authentication ?? new SimpleDataAccessLayer.Common.config.models.old.WindowsAuthentication()) is
                                SimpleDataAccessLayer.Common.config.models.old.WindowsAuthentication)
                                ? new config.models.Authentication { AuthenticationType = AuthenticationType.WindowsAuthentication}
                                : new config.models.Authentication
                                {
                                    AuthenticationType = AuthenticationType.SqlAuthentication,
                                    SavePassword = true,
                                    UserName =
                                        ((config.models.old.SqlAuthentication) config.DesignerConnection?.Authentication)
                                            ?.UserName,
                                    Password =
                                        ((config.models.old.SqlAuthentication) config.DesignerConnection?.Authentication)
                                            ?.Password
                                },
                        DatabaseName = csb.InitialCatalog,
                        ServerName = csb.DataSource

                    },
                    RuntimeConnectionStringName = config.ApplicationConnectionString,
                    Namespace = config.Namespace,
                    Enums = config.Enums?.Select(e => new SimpleDataAccessLayer.Common.config.models.Enum()
                    {
                        Schema = e.Schema,
                        Alias = e.Alias,
                        ValueColumn = e.ValueColumn,
                        KeyColumn = e.KeyColumn,
                        TableName = e.TableName
                    }).ToList(),
                    Procedures = config.Procedures?.Select(p => new Procedure()
                    {
                        Alias = p.Alias,
                        Schema = p.Schema,
                        ProcedureName = p.ProcedureName
                    }).ToList()
                };
            }
            catch (Exception e)
            {
                throw new DalConfigXmlConverterException("Failed to parse DalConfig XML", e);
            }
        }
    }
}
