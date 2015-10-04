using System.Collections.Generic;
using System.Linq;

namespace SimpleDataAccessLayer.Common.config.models
{
    //

    public class DalConfig
    {

        public DesignerConnection DesignerConnection { get; set; }

        public string Namespace { get; set; }

        public string RuntimeConnectionStringName { get; set; }

        public List<Enum> Enums { get; set; }

        public List<Procedure> Procedures { get; set; }

        public DalConfig()
        {
            DesignerConnection = new DesignerConnection();
            Namespace = "";
            Enums = new List<SimpleDataAccessLayer.Common.config.models.Enum>();
            Procedures = new List<Procedure>();
            RuntimeConnectionStringName = "";
        }

        public DalConfig Clone()
        {
            var clone = new DalConfig()
            {
                DesignerConnection = new DesignerConnection()
                {
                    Authentication = new Authentication()
                    {
                        AuthenticationType =
                            (DesignerConnection?.Authentication?.AuthenticationType) ??
                            AuthenticationType.WindowsAuthentication,
                        Password =
                            DesignerConnection?.Authentication?.Password == null
                                ? null
                                : string.Copy(DesignerConnection?.Authentication?.Password),
                        UserName =
                            DesignerConnection?.Authentication?.UserName == null
                                ? null
                                : string.Copy(DesignerConnection?.Authentication?.UserName),
                        SavePassword = (DesignerConnection?.Authentication?.SavePassword) ?? false
                    },
                    DatabaseName =
                        DesignerConnection?.DatabaseName == null ? null : string.Copy(DesignerConnection?.DatabaseName),
                    ServerName =
                        DesignerConnection?.ServerName == null ? null : string.Copy(DesignerConnection?.ServerName)
                },
                Namespace = Namespace == null ? null : string.Copy(Namespace),
                RuntimeConnectionStringName =
                    RuntimeConnectionStringName == null ? null : string.Copy(RuntimeConnectionStringName),
                Enums = new List<Enum>(Enums.Select(e => new Enum
                {
                    Alias = e.Alias == null ? null : string.Copy(e.Alias),
                    Schema = e.Schema == null ? null : string.Copy(e.Schema),
                    ValueColumn = e.ValueColumn == null ? null : string.Copy(e.ValueColumn),
                    KeyColumn = e.KeyColumn == null ? null : string.Copy(e.KeyColumn),
                    TableName = e.TableName == null ? null : string.Copy(e.TableName)
                })),
                Procedures = new List<Procedure>(Procedures.Select(p => new Procedure
                {
                    Schema = p.Schema == null ? null : string.Copy(p.Schema),
                    Alias = p.Alias == null ? null : string.Copy(p.Alias),
                    ProcedureName = p.ProcedureName == null ? null : string.Copy(p.ProcedureName)
                }))
            };

            return clone;
        }
    }
}
