using System;
using System.Collections.Generic;
using CHC.Consent.Common.Infrastructure;
using JetBrains.Annotations;

namespace CHC.Consent.Api.Infrastructure.Web
{
    /// <inheritdoc />
    /// <summary>
    /// Add types from a <see cref="T:CHC.Consent.Common.Infrastructure.ITypeRegistry" /> to the Swagger 2.0 documentation 
    /// </summary>
    /// <remarks>OpenAPI 3.0 seems to change the way this works</remarks>
    [UsedImplicitly]
    public class SwaggerSchemaIdentityTypeProvider<TIdentifier, TIdentifierRegistry> : SwaggerSchemaSubtypeFilter<TIdentifier>
        where TIdentifierRegistry:ITypeRegistry
    {
        public SwaggerSchemaIdentityTypeProvider(TIdentifierRegistry registry):
            base(registry.RegisteredTypes, registry.RegisteredNames)
        {
        }
    }
}