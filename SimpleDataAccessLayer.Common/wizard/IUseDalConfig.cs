using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.wizard
{
    internal interface IUseDalConfig
    {
        void UpdateDalConfig(DalConfig config);
        void InitializeFromDalConfig(DalConfig config);
    }
}
