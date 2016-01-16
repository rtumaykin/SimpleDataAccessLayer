using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using SimpleDataAccessLayer.Common.codegen;
using SimpleDataAccessLayer.Common.config.models.old;
using SimpleDataAccessLayer.Common.utilities;
using SimpleDataAccessLayer.Common.wizard;
using AuthenticationType = SimpleDataAccessLayer.Common.config.models.AuthenticationType;
using Enum = SimpleDataAccessLayer.Common.config.models.Enum;
using Formatting = Newtonsoft.Json.Formatting;
using Procedure = SimpleDataAccessLayer.Common.config.models.old.Procedure;

namespace UnitTests
{
    [TestFixture]
    public class Tests
    {


        [Test]
        public void Should_Convert_Successfully_Random()
        {
            for (var i = 0; i < 100000; i++)
            {
                var xmlConfig = GenerateConfig();
                var serializedConfig = SerializeXmlConfig(xmlConfig);
                var newConfig = DalConfigXmlConverter.ParseDalConfigXml(serializedConfig);

                if (!CompareConfigs(xmlConfig, newConfig))
                {
                    //
                }


                Assert.IsTrue(CompareConfigs(xmlConfig, newConfig));
            }
        }

        [Test]
        public void should_successfully_serialize_deserialize_config()
        {
            var config = new SimpleDataAccessLayer.Common.config.models.DalConfig()
            {
                DesignerConnection = new SimpleDataAccessLayer.Common.config.models.DesignerConnection()
                {
                    Authentication = new SimpleDataAccessLayer.Common.config.models.Authentication()
                    {
                        AuthenticationType = AuthenticationType.WindowsAuthentication
                    },
                    ServerName = "sql-2012",
                    DatabaseName = "ras_sas"
                },
                Namespace = "kjskjs",
                RuntimeConnectionStringName = "skjskjsks",
                Enums = new List<Enum>(new[]
                {
                    new Enum()
                    {
                        Schema = "sasds",
                        Alias = "skjsksj",
                        ValueColumn = "slslsk",
                        KeyColumn = "skss",
                        TableName = "sjhsjshsj"
                    }
                }),
                Procedures = new List<SimpleDataAccessLayer.Common.config.models.Procedure>()
            };

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());

            var serializedConfig = JsonConvert.SerializeObject(config, Formatting.Indented, settings);

            var deserializedConfig = JsonConvert.DeserializeObject<SimpleDataAccessLayer.Common.config.models.DalConfig>(serializedConfig, settings);



        }

        [Test]
        [Ignore("")]
        [STAThread]
        public void RunVisual()
        {
            var config = new SimpleDataAccessLayer.Common.config.models.DalConfig()
            {
                DesignerConnection = new SimpleDataAccessLayer.Common.config.models.DesignerConnection()
                {
                    Authentication = new SimpleDataAccessLayer.Common.config.models.Authentication()
                    {
                        AuthenticationType = AuthenticationType.SqlAuthentication,
                        UserName = "sa",
                        Password = "somePass",
                        SavePassword = true
                    },
                    ServerName = "sql-2012"
                }
            };
            var viz = new ModelWizard(config);
            viz.ShowDialog();
            if (viz.DialogResult == DialogResult.OK)
            {
                var code = new Main(config).GetCode();
            }
        }

        private static bool CompareConfigs(DalConfig xmlConfig, SimpleDataAccessLayer.Common.config.models.DalConfig newConfig)
        {
            if (xmlConfig.ApplicationConnectionString != newConfig.RuntimeConnectionStringName)
                return false;

            if (xmlConfig.Namespace != newConfig.Namespace)
                return false;

            // enums
            if (xmlConfig.Enums == null && newConfig.Enums != null)
                return false;

            if (xmlConfig.Enums != null && newConfig.Enums == null)
                return false;

            if (xmlConfig.Enums != null && newConfig.Enums != null && xmlConfig.Enums.Count != newConfig.Enums.Count)
                return false;

            if (xmlConfig.Enums != null && newConfig.Enums != null &&
                xmlConfig.Enums.Select(
                    xmlConfigEnum =>
                        newConfig.Enums.FirstOrDefault(
                            newConfigEnum =>
                                newConfigEnum.Schema == xmlConfigEnum.Schema &&
                                newConfigEnum.Alias == xmlConfigEnum.Alias &&
                                newConfigEnum.KeyColumn == xmlConfigEnum.KeyColumn &&
                                newConfigEnum.TableName == xmlConfigEnum.TableName &&
                                newConfigEnum.ValueColumn == xmlConfigEnum.ValueColumn))
                    .Any(newConfigMatchEnum => newConfigMatchEnum == null))
                return false;

            if (xmlConfig.Procedures != null && newConfig.Procedures != null &&
                xmlConfig.Procedures.Select(
                    xmlConfigProcedure =>
                        newConfig.Procedures.FirstOrDefault(
                            newConfigProcedure =>
                                newConfigProcedure.Schema == xmlConfigProcedure.Schema &&
                                newConfigProcedure.Alias == xmlConfigProcedure.Alias &&
                                newConfigProcedure.ProcedureName == xmlConfigProcedure.ProcedureName))
                    .Any(newConfigMatchProcedure => newConfigMatchProcedure == null))
                return false;

            if (xmlConfig.DesignerConnection != null && newConfig.DesignerConnection == null)
                return false;

            if (xmlConfig.DesignerConnection == null && newConfig.DesignerConnection != null &&
                newConfig.DesignerConnection.Authentication.AuthenticationType == AuthenticationType.SqlAuthentication)
                return false;

            if (xmlConfig.DesignerConnection != null && newConfig.DesignerConnection != null)
            {
                if (xmlConfig.DesignerConnection.Authentication is WindowsAuthentication &&
                    newConfig.DesignerConnection.Authentication.AuthenticationType == AuthenticationType.SqlAuthentication)
                    return false;
                if (xmlConfig.DesignerConnection.Authentication is SimpleDataAccessLayer.Common.config.models.old.SqlAuthentication &&
                    newConfig.DesignerConnection.Authentication.AuthenticationType == AuthenticationType.WindowsAuthentication)
                    return false;
                if (xmlConfig.DesignerConnection.Authentication is SimpleDataAccessLayer.Common.config.models.old.SqlAuthentication &&
                      newConfig.DesignerConnection.Authentication.AuthenticationType == AuthenticationType.SqlAuthentication
                      &&
                      !(((SimpleDataAccessLayer.Common.config.models.old.SqlAuthentication) xmlConfig.DesignerConnection.Authentication)
                          .UserName == newConfig.DesignerConnection.Authentication.UserName &&
                    ((SimpleDataAccessLayer.Common.config.models.old.SqlAuthentication) xmlConfig.DesignerConnection.Authentication)
                        .Password == newConfig.DesignerConnection.Authentication.Password))
                return false;

            }

            return true;
        }


        // helpers
        private string SerializeXmlConfig(DalConfig config)
        {
            var ser = new DataContractSerializer(typeof(DalConfig));
            var settings = new XmlWriterSettings { Indent = true, Encoding = Encoding.Unicode };

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                ser.WriteObject(writer, config);
            }

            return sb.ToString();
        }

        private DalConfig GenerateConfig()
        {
            var rnd = new Random();

            var config = new DalConfig()
            {
                ApplicationConnectionString = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N"),
                Namespace = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N"),
                DesignerConnection = rnd.Next(0, 9) < 2
                    ? null
                    : new DesignerConnection()
                    {
                        Authentication =
                            rnd.Next(0, 9) < 2
                                ? null
                                : (rnd.Next(0, 9) < 5
                                    ? new WindowsAuthentication() as
                                        Authentication
                                    : new SimpleDataAccessLayer.Common.config.models.old.SqlAuthentication(
                                        rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N"),
                                        rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N")))
                    },
                Enums = GetEnums(),
                Procedures = GetProcedures()
            };
            

            return config;
        }


        private List<Procedure> GetProcedures()
        {
            var rnd = new Random();
            var ret = new List<Procedure>();

            if (rnd.Next(0, 10) > 3)
            {
                for (var i = 0; i < rnd.Next(1, 10); i++)
                {
                    ret.Add(new Procedure()
                    {
                        Schema = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N"),
                        ProcedureName = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N"),
                        Alias = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N")
                    });
                }
            }

            return ret;
        }

        private List<SimpleDataAccessLayer.Common.config.models.old.Enum> GetEnums()
        {

            var rnd = new Random();
            var ret = new List<SimpleDataAccessLayer.Common.config.models.old.Enum>();

            if (rnd.Next(0, 10) > 3)
            {
                for (var i = 0; i < rnd.Next(1, 10); i++)
                {
                    ret.Add(new SimpleDataAccessLayer.Common.config.models.old.Enum()
                    {
                        Schema = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N"),
                        Alias = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N"),
                        ValueColumn = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N"),
                        KeyColumn = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N"),
                        TableName = rnd.Next(0, 9) < 2 ? null : Guid.NewGuid().ToString("N")
                    });
                }
            }

            return ret;
        }
    }
}

