using static SimpleDataAccessLayer.Common.codegen.Tools;

namespace SimpleDataAccessLayer.Common.codegen
{
    public class ProcedureInfo
    {
        public string ObjectSchemaName { get; }

        public string ObjectName { get; }

        public string FullObjectName => QuoteName(ObjectSchemaName) + "." + QuoteName(ObjectName);

        public string Alias { get; private set; }

        public ProcedureInfo(string objectSchemaName, string objectName, string alias)
        {
            ObjectName = objectName;
            ObjectSchemaName = objectSchemaName;
            Alias = alias;
        }
    }
}
