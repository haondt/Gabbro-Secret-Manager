using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GabbroSecretManager.Domain.Api.Models
{
    public class ApiConstants
    {
        public static JsonSerializerSettings SerializerSettings { get; set; }

        static ApiConstants()
        {
            SerializerSettings = new();
            SerializerSettings.TypeNameHandling = TypeNameHandling.None;
            SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            SerializerSettings.Formatting = Formatting.None;
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false,
                    OverrideSpecifiedNames = true
                }
            };
        }
    }
}
