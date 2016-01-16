using System.Configuration;
using System.Data.SqlClient;
using SimpleDataAccessLayer.Common.config.models;
using SimpleDataAccessLayer.Common.wizard;

namespace SimpleDataAccessLayer.Common.codegen
{
#if DEBUG
    public class Main
#else
    public class Main
#endif
    {
        public Main(DalConfig dalConfig)
        {
            _config = dalConfig;
        }

        private readonly DalConfig _config;


        private string _designerConnectionString;

        public string DesignerConnectionString
        {
            get
            {
                System.Threading.LazyInitializer.EnsureInitialized(
                    ref _designerConnectionString,
                    () => Utilities.BuildConnectionString(_config));
                return _designerConnectionString;
            }
        }


        public string GetCode()
        {
            try
            {
                ISqlRepository sqlRepository = new SqlRepository(DesignerConnectionString);

                return
                    $"{new Common(_config).GetCode()}{new TableValuedParameter(_config, sqlRepository).GetCode()}{new Enum(_config, sqlRepository).GetCode()}{new Procedure(_config, sqlRepository).GetCode()}";
            }
            catch
            {
                return "/* Unable to generate code due to empty or invalid configuration */";
            }
        }
    }
}
