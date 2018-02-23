using System;
using System.Collections.Generic;
using System.Linq;

using CHC.Consent.Common.Infrastructure;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api.Infrastructure
{
    /// <summary>
    /// Add types from a <see cref="ITypeRegistry"/> to the Swagger 2.0 documentation 
    /// </summary>
    /// <remarks>OpenAPI 3.0 seems to change the way this works</remarks>
    public class SwaggerSchemaIdentityTypeProvider<TIdentifier, TIdentifierRegistry> : ISchemaFilter
        where TIdentifierRegistry:ITypeRegistry
    {
        private readonly TIdentifierRegistry  registry;

        public SwaggerSchemaIdentityTypeProvider(TIdentifierRegistry registry)
        {
            this.registry = registry;            
        }

        public void Apply(Schema model, SchemaFilterContext context)
        {   
            if(!typeof(TIdentifier).IsAssignableFrom(context.SystemType)) return;
            if (context.SystemType == typeof(TIdentifier))
            {
                
                model.Discriminator = "$type";
                model.Properties.Add(
                    "$type",
                    new Schema {Enum = registry.Select(_ => /*$"#/definitions/{_.Name}"*/_.Name).Cast<object>().ToArray(), Type = "string"});
                if(model.Required == null) model.Required = new List<string>();
                model.Required.Add("$type");

                foreach (var identifierRegistration in registry)
                {
                    context.SchemaRegistry.GetOrRegister(identifierRegistration.Type);
                }
            }
            else
            {
                var schema = new Schema {Properties = model.Properties, Type = model.Type, Required = model.Required};
                model.AllOf = new[] { context.SchemaRegistry.GetOrRegister(typeof(TIdentifier)), schema };
                model.Properties = null;
                model.Required = null;
                model.Type = null;
            }
        }
    }
}