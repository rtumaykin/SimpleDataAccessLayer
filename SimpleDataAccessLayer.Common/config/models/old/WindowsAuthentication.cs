using System.Runtime.Serialization;

namespace SimpleDataAccessLayer.Common.config.models.old
{
    [DataContract(Namespace = "RomanTumaykin.SimpleDataAcessLayer", Name = "WindowsAuthentication")]
    public class WindowsAuthentication : Authentication
    {
        public WindowsAuthentication()
            : base(AuthenticationType.Windows)
        { }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WindowsAuthentication)obj);
        }

        protected bool Equals(WindowsAuthentication other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return 1;
        }
    }
}