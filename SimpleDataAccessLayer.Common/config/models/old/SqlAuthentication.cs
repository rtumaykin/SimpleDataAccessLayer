using System.Runtime.Serialization;

namespace SimpleDataAccessLayer.Common.config.models.old
{
    [DataContract(Namespace = "RomanTumaykin.SimpleDataAcessLayer", Name = "SqlAuthentication")]
    public class SqlAuthentication : Authentication
    {
        [DataMember(IsRequired = true)]
        public string UserName { get; set; }

        [DataMember(IsRequired = true)]
        public string Password { get; set; }

        public SqlAuthentication(string userName, string password)
            : base(AuthenticationType.Sql)
        {
            UserName = userName;
            Password = password;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SqlAuthentication)obj);
        }

        protected bool Equals(SqlAuthentication other)
        {
            return base.Equals(other) && string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (UserName != null ? UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}