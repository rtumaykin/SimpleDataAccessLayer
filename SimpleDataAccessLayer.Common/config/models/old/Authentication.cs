using System.Runtime.Serialization;

namespace SimpleDataAccessLayer.Common.config.models.old
{
    //
    [DataContract(Namespace = "RomanTumaykin.SimpleDataAcessLayer", Name = "Authentication")]
    [KnownType(typeof(SqlAuthentication))]
    [KnownType(typeof(WindowsAuthentication))]
    public abstract class Authentication
    {
        private AuthenticationType _type;
        public Authentication(AuthenticationType type)
        {
            _type = type;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Authentication)obj);
        }

        protected bool Equals(Authentication other)
        {
            return _type == other._type;
        }

        public override int GetHashCode()
        {
            return (int)_type;
        }
    }
}
