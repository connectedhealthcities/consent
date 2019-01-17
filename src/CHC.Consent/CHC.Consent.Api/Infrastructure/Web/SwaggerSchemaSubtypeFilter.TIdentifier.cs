using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api.Infrastructure.Web
{
    /// <summary>
    /// Add subtypes to the Swagger 2.0 documentation 
    /// </summary>
    /// <remarks>OpenAPI 3.0 seems to change the way this works</remarks>
    public class SwaggerSchemaSubtypeFilter<TBaseType> : ISchemaFilter
    {
        /// <inheritdoc />
        public SwaggerSchemaSubtypeFilter(IEnumerable<Type> subTypes, IEnumerable<string> typeNames)
        {
            SubTypes = subTypes;
            TypeNames = typeNames;
        }

        public void Apply(Schema model, SchemaFilterContext context)
        {   
            if(!IsBaseOrSubtype(context.SystemType)) return;
            if (context.SystemType == typeof(TBaseType))
            {
                model.Discriminator = "$type";
                model.Properties.Add(
                    "$type",
                    new Schema {Enum = TypeNames.Cast<object>().ToArray(), Type = "string"});
                if(model.Required == null) model.Required = new List<string>();
                model.Required.Add("$type");

                foreach (var identifierType in SubTypes)
                {

                    try
                    {
                        context.SchemaRegistry.GetOrRegister(identifierType);
                    }
                    catch (ArgumentException e)
                    {
                        throw new InvalidOperationException($"Cannot included {identifierType} in schema - see inner exception for details", e);
                    }
                }
            }
            else
            {
                var schema = new Schema {Properties = model.Properties, Type = model.Type, Required = model.Required};
                model.AllOf = new[] { context.SchemaRegistry.GetOrRegister(typeof(TBaseType)), schema };
                model.Extensions["x-ms-client-name"] = context.SystemType.FriendlyId();
                model.Extensions["x-ms-discriminator-value"] = context.SystemType.FriendlyId();
                model.Properties = null;
                model.Required = null;
                model.Type = null;
            }
        }

        protected static bool IsBaseOrSubtype(Type type)
        {
            return typeof(TBaseType).IsAssignableFrom(type);
        }

        private IEnumerable<Type> SubTypes { get; }
        private IEnumerable<string> TypeNames { get; }
    }
}