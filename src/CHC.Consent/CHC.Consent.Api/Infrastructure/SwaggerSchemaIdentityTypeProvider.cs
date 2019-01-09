using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using JetBrains.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api.Infrastructure
{
    /// <summary>
    /// Add subtypes to the Swagger 2.0 documentation 
    /// </summary>
    /// <remarks>OpenAPI 3.0 seems to change the way this works</remarks>
    public abstract class SwaggerSchemaSubtypeFilter<TIdentifier> : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {   
            if(!IsBaseOrSubtype(context.SystemType)) return;
            if (context.SystemType == typeof(TIdentifier))
            {
                model.Discriminator = "$type";
                model.Properties.Add(
                    "$type",
                    new Schema {Enum = TypeNames.Cast<object>().ToArray(), Type = "string"});
                if(model.Required == null) model.Required = new List<string>();
                model.Required.Add("$type");

                foreach (var identifierType in SubTypes())
                {
                    context.SchemaRegistry.GetOrRegister(identifierType);
                }
            }
            else
            {
                var schema = new Schema {Properties = model.Properties, Type = model.Type, Required = model.Required};
                model.AllOf = new[] { context.SchemaRegistry.GetOrRegister(typeof(TIdentifier)), schema };
                model.Extensions["x-ms-client-name"] = context.SystemType.Name;
                model.Properties = null;
                model.Required = null;
                model.Type = null;
            }
        }

        protected static bool IsBaseOrSubtype(Type type)
        {
            return typeof(TIdentifier).IsAssignableFrom(type);
        }

        protected abstract IEnumerable<Type> SubTypes();
        protected abstract IEnumerable<string> TypeNames { get; }
    }
    
    /// <inheritdoc />
    /// <summary>
    /// Add types from a <see cref="T:CHC.Consent.Common.Infrastructure.ITypeRegistry" /> to the Swagger 2.0 documentation 
    /// </summary>
    /// <remarks>OpenAPI 3.0 seems to change the way this works</remarks>
    [UsedImplicitly]
    public class SwaggerSchemaIdentityTypeProvider<TIdentifier, TIdentifierRegistry> : SwaggerSchemaSubtypeFilter<TIdentifier>
        where TIdentifierRegistry:ITypeRegistry
    {
        private readonly TIdentifierRegistry  registry;

        public SwaggerSchemaIdentityTypeProvider(TIdentifierRegistry registry)
        {
            this.registry = registry;
        }

        protected override IEnumerable<Type> SubTypes() => registry.RegisteredTypes;

        protected override IEnumerable<string> TypeNames => registry.RegisteredNames;
    }

    /// <inheritdoc />
    /// <summary>
    /// Add Identifier Types to the Swagger 2.0 documentation 
    /// </summary>
    /// <remarks>OpenAPI 3.0 seems to change the way this works</remarks>
    [UsedImplicitly]
    public class SwaggerSchemaIdentityTypeProvider : SwaggerSchemaSubtypeFilter<IIdentifierType>
    {

        private readonly Type[] identifierTypes;

        public SwaggerSchemaIdentityTypeProvider()
        {
            identifierTypes = Assembly.GetEntryAssembly().GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(a => a.ExportedTypes)
                .Where(IsBaseOrSubtype)
                .Where(type => type != typeof(IIdentifierType))
                .ToArray();
        }


        /// <inheritdoc />
        protected override IEnumerable<Type> SubTypes() => identifierTypes;

        /// <inheritdoc />
        protected override IEnumerable<string> TypeNames => identifierTypes.Select(_ => _.Name);
    }
}