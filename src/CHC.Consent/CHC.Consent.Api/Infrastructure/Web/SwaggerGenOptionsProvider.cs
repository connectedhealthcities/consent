﻿using System;
using System.Collections.Generic;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api.Infrastructure.Web
{
    public class SwaggerGenOptionsProvider : IConfigureOptions<SwaggerGenOptions>
    {
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public SwaggerGenOptionsProvider(IServiceProvider services)
        {
            
            Services = services;
        }

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions gen)
        {
            gen.AddSecurityDefinition(
                "oauth2",
                new OAuth2Scheme
                {
                    Flow = "implicit",
                    AuthorizationUrl = "http://localhost:5000/connect/authorize",
                    Scopes = new Dictionary<string, string>
                    {
                        ["read"] = "read",
                        ["write"] = "write"
                    }
                });
            gen.SwaggerDoc("v1", new Info {Title = "Api", Version = "1"});
            gen.DescribeAllEnumsAsStrings();
            gen.SchemaFilter<SwaggerSchemaIdentityTypeProvider>();
            gen.SchemaFilter<SwaggerSchemaIdentityTypeProvider<CaseIdentifier, ITypeRegistry<CaseIdentifier>>>();
            gen.SchemaFilter<SwaggerSchemaIdentityTypeProvider<Evidence, ITypeRegistry<Evidence>>>();
            gen.CustomSchemaIds(t => GetSchemaId(t)?? t.FriendlyId(fullyQualified:false));
        }

        private string GetSchemaId(Type type)
        {
            return TryGet<Type, string>(
                type,
                /*Services.GetRequiredService<ITypeRegistry<IPersonIdentifier>>().TryGetName,*/
                Services.GetRequiredService<ITypeRegistry<CaseIdentifier>>().TryGetName,
                Services.GetRequiredService<ITypeRegistry<Evidence>>().TryGetName
            );
        }

        delegate bool TryGetValue<in TKey, TValue>(TKey key, out TValue value);

        static TValue TryGet<TKey, TValue>(TKey key, params TryGetValue<TKey, TValue>[] getters)
        {
            foreach (var tryGetValue in getters)
            {
                if (tryGetValue(key, out var value)) return value;
            }

            return default;
        }
    }
}