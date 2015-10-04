using System.Runtime.Serialization;

namespace SimpleDataAccessLayer.Common.config.models.old
{
    [DataContract(Namespace = "RomanTumaykin.SimpleDataAcessLayer", Name = "Enum")]
    public class Enum
    {
        [DataMember(IsRequired = true)]
        public string Schema { get; set; }
        [DataMember(IsRequired = true)]
        public string TableName { get; set; }
        [DataMember(IsRequired = true)]
        public string KeyColumn { get; set; }
        [DataMember(IsRequired = true)]
        public string ValueColumn { get; set; }
        [DataMember(IsRequired = true)]
        public string Alias { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Enum)obj);
        }

        protected bool Equals(Enum other)
        {
            return string.Equals(Schema, other.Schema) && string.Equals(TableName, other.TableName) && string.Equals(KeyColumn, other.KeyColumn) && string.Equals(ValueColumn, other.ValueColumn) && string.Equals(Alias, other.Alias);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Schema != null ? Schema.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TableName != null ? TableName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (KeyColumn != null ? KeyColumn.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ValueColumn != null ? ValueColumn.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Alias != null ? Alias.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}