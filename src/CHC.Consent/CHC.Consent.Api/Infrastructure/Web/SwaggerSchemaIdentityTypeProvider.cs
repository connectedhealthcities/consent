using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CHC.Consent.Common.Identity.Identifiers;
using JetBrains.Annotations;

namespace CHC.Consent.Api.Infrastructure.Web
{
    /// <inheritdoc />
    /// <summary>
    /// Add Identifier Types to the Swagger 2.0 documentation 
    /// </summary>
    /// <remarks>OpenAPI 3.0 seems to change the way this works</remarks>
    [UsedImplicitly]
    public class SwaggerSchemaIdentityTypeProvider : SwaggerSchemaSubtypeFilter<IIdentifierType>
    {
        private static readonly Lazy<Type[]> subtypes;

        static SwaggerSchemaIdentityTypeProvider()
        {
            subtypes = new Lazy<Type[]>(() => Assembly.GetEntryAssembly().GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(a => a.ExportedTypes)
                .Where(IsBaseOrSubtype)
                .Where(type => type != typeof(IIdentifierType))
                .ToArray());
        }

        public SwaggerSchemaIdentityTypeProvider() : base(subtypes.Value, subtypes.Value.Select(_ => _.Name))
        {
        }
    }
}