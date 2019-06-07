using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure
{
    public static class SerializationBinderExtensions
    {
        public static JsonSerializerSettings AddSwashbuckleNamesBinderFor<T>(this JsonSerializerSettings settings)
        {
            settings.SerializationBinder =
                new DerivedTypeSerializationBinder<T>(settings.SerializationBinder ?? new DefaultSerializationBinder());
            return settings;
        }
    }
}