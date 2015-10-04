namespace SimpleDataAccessLayer.Common.config.models
{
    public class Authentication
    {
        public AuthenticationType AuthenticationType { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool SavePassword { get; set; }

    }
}