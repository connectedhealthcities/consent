using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;
using Microsoft.Extensions.Configuration;
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
            var oAuth2Options = Services.GetRequiredService<IConfiguration>()
                .GetSection("IdentityServer")
                .Get<IdentityServerConfiguration>();
            
            gen.AddSecurityDefinition(
                "oauth2",
                new OAuth2Scheme
                {
                    Flow = "implicit",
                    AuthorizationUrl =  new UriBuilder(oAuth2Options.Authority) { Path = "connect/authorize" }.Uri.ToString(),
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
            
            gen.SchemaFilter<EnumSchemaFilter>();

            var definitionTypes = new [] { typeof(IdentifierDefinition), typeof(EvidenceDefinition) };
            gen.SchemaFilter<SwaggerSchemaSubtypeFilter<IDefinition>>(
                definitionTypes,
                definitionTypes.Select(_ => _.FriendlyId()));

            var matchSpecificationTypes = Assembly.GetEntryAssembly().Modules
                .SelectMany(m => m.GetTypes())
                .Where(_ => _.IsSubtypeOf(typeof(MatchSpecification)) && _.IsConcreteType())
                .ToArray();

            gen.SchemaFilter<SwaggerSchemaSubtypeFilter<MatchSpecification>>(
                matchSpecificationTypes,
                matchSpecificationTypes.Select(_ => _.Name.ToLowerCamel()));
        }
    }

    public class EnumSchemaFilter : ISchemaFilter
    {
        /// <inheritdoc />
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            if (!context.SystemType.IsEnum) return;
            schema.Extensions.Add(
                "x-ms-enum",
                new
                {
                    name = context.SystemType.Name,
                    modelAsString = false
                });
        }
    }
}