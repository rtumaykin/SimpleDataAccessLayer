namespace SimpleDataAccessLayer.Common.codegen
{
    public class TableType
    {
        public string SchemaName { get; private set; }
        public string Name { get; private set; }

        public TableType(string schemaName, string name)
        {
            SchemaName = schemaName;
            Name = name;
        }
    }
}