using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SimpleDataAccessLayer.Common.config.models.old
{
    [DataContract(Namespace = "RomanTumaykin.SimpleDataAcessLayer", Name = "DalConfig")]
    [KnownType(typeof(Enum))]
    [KnownType(typeof(Authentication))]
    [KnownType(typeof(DesignerConnection))]
    [KnownType(typeof(WindowsAuthentication))]
    [KnownType(typeof(SqlAuthentication))]
    public class DalConfig
    {
        [DataMember(IsRequired = true)]
        public DesignerConnection DesignerConnection { get; set; }
        [DataMember(IsRequired = true)]
        public String Namespace { get; set; }
        [DataMember(IsRequired = true)]
        public String ApplicationConnectionString { get; set; }
        [XmlElement("Enums")]
        [DataMember(IsRequired = true)]
        public List<Enum> Enums { get; set; }
        [XmlElement("Procedures")]
        [DataMember(IsRequired = true)]
        public List<Procedure> Procedures { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DalConfig)obj);
        }

        protected bool Equals(DalConfig other)
        {
            return Equals(DesignerConnection, other.DesignerConnection) && string.Equals(Namespace, other.Namespace) &&
                   string.Equals(ApplicationConnectionString, other.ApplicationConnectionString) &&
                   Equals(Enums, other.Enums) && Equals(Procedures, other.Procedures);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (DesignerConnection != null ? DesignerConnection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ApplicationConnectionString != null ? ApplicationConnectionString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Enums != null ? Enums.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Procedures != null ? Procedures.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}