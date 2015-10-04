using System;
using System.Linq;
using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.codegen
{
#if DEBUG 
    public class Enum
#else 
    internal class Enum
#endif
    {
        private readonly DalConfig _config;
        private readonly ISqlRepository _sqlRepository;

        public Enum(DalConfig config, ISqlRepository sqlRepository)
        {
            _config = config;
            if (sqlRepository == null)
                throw new ArgumentNullException(nameof(sqlRepository));
            _sqlRepository = sqlRepository;
        }

        public string GetCode()
        {
            if (_config?.Enums == null)
                return "";

            return string.Join("", _config.Enums.Select(e => e.Schema)
                .Distinct().Select(ns =>
                    $"namespace {_config.Namespace}.Enums.{ns} {{{GetEnumsCodeForNamespace(ns)}}}"));

        }

        private string GetEnumsCodeForNamespace(string ns)
        {
            return string.Join("",
                _config.Enums.Where(e => e.Schema == ns)
                    .Select(
                        e =>
                            $"public enum {Tools.ValidIdentifier(string.IsNullOrWhiteSpace(e.Alias) ? e.TableName : e.Alias)}{{{GetEnumBodyCode(e)}}}"));
        }

        private string GetEnumBodyCode(config.models.Enum enumInfo)
        {
            return string.Join(",",
                _sqlRepository.GetEnumKeyValues(enumInfo.Schema, enumInfo.TableName, enumInfo.ValueColumn, enumInfo.KeyColumn)
                    .Select(kv => $"{Tools.ValidIdentifier(kv.Key)} = {kv.Value}"));
        }
    }
}
