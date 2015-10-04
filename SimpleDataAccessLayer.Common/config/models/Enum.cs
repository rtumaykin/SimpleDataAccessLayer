namespace SimpleDataAccessLayer.Common.config.models
{
    public class Enum
    {
        public string Schema { get; set; }

        public string TableName { get; set; }

        public string KeyColumn { get; set; }

        public string ValueColumn { get; set; }

        public string Alias { get; set; }
    }
}