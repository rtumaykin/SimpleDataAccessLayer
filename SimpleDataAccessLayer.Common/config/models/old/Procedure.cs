using System.Runtime.Serialization;

namespace SimpleDataAccessLayer.Common.config.models.old
{
    [DataContract(Namespace = "RomanTumaykin.SimpleDataAcessLayer", Name = "Procedure")]
    public class Procedure
    {
        [DataMember(IsRequired = true)]
        public string Schema { get; set; }
        [DataMember(IsRequired = true)]
        public string ProcedureName { get; set; }
        [DataMember(IsRequired = false)]
        public string Alias { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Procedure)obj);
        }

        protected bool Equals(Procedure other)
        {
            return string.Equals(Schema, other.Schema) && string.Equals(ProcedureName, other.ProcedureName) && string.Equals(Alias, other.Alias);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Schema != null ? Schema.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ProcedureName != null ? ProcedureName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Alias != null ? Alias.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}