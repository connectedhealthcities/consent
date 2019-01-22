using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CHC.Consent.Api.Infrastructure
{
    public class ConfigureJsonOptions : IConfigureOptions<MvcJsonOptions>
    {
        public readonly IServiceProvider services;

        /// <inheritdoc />
        public ConfigureJsonOptions(IServiceProvider services)
        {
            this.services = services;
        }

        /// <inheritdoc />
        public void Configure(MvcJsonOptions options)
        {
            ConfigureSerializer(options.SerializerSettings, services.GetService<ILogger<IIdentifierValueDtoJsonConverter>>());
        }

        public static JsonSerializerSettings ConfigureSerializer(
            JsonSerializerSettings settings,
            ILogger<IIdentifierValueDtoJsonConverter> logger = null)
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.SerializationBinder = new IdentifierTypeSerializationBinder(); 
            settings.Converters.Add(
                new IIdentifierValueDtoJsonConverter(logger ?? NullLogger<IIdentifierValueDtoJsonConverter>.Instance));
            return settings;
        }
    }
}