using System.Runtime.Serialization;

namespace SimpleDataAccessLayer.Common.config.models.old
{
    [DataContract(Namespace = "RomanTumaykin.SimpleDataAcessLayer", Name = "DesignerConnection")]
    public class DesignerConnection
    {
        [DataMember]
        public Authentication Authentication { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DesignerConnection)obj);
        }

        protected bool Equals(DesignerConnection other)
        {
            return Equals(Authentication, other.Authentication);
        }

        public override int GetHashCode()
        {
            return (Authentication != null ? Authentication.GetHashCode() : 0);
        }
    }
}