using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions;
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
                        ["openid"] = "Open id (Required)",
                        ["profile"] = "Profile",
                        ["api"] = "Api"
                    }
                });
            gen.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {["oauth2"] = new[] {"api"}});
            gen.SwaggerDoc("v1", new Info {Title = "Api", Version = "1"});
            gen.DescribeAllEnumsAsStrings();
            gen.SchemaFilter<SwaggerSchemaIdentityTypeProvider>();
            gen.SchemaFilter<SwaggerSchemaSubtypeFilter<IIdentifierValueDto>>(
                IdentifierValueDtos.KnownDtoTypes,
                IdentifierValueDtos.KnownDtoTypes.Select(_ => _.FriendlyId()));

            var definitionTypes = new [] { typeof(IdentifierDefinition), typeof(EvidenceDefinition) };
            gen.SchemaFilter<SwaggerSchemaSubtypeFilter<IDefinition>>(
                definitionTypes,
                definitionTypes.Select(_ => _.FriendlyId()));
        }
    }

    
}