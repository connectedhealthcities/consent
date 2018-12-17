using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Swashbuckle.AspNetCore.Swagger;

namespace CHC.Consent.Api.Infrastructure
{
    public class IdentityDefinitionSwaggerSchemaGenerator : IIdentifierDefinitionVisitor
    {
        private IDictionary<string, Schema> Schemas { get; }

        /// <inheritdoc />
        public IdentityDefinitionSwaggerSchemaGenerator(IDictionary<string, Schema> schemas)
        {
            Schemas = schemas;
        }
        
        private Schema currentSchema;
        /// <inheritdoc />
        public void Visit(DateIdentifierType type)
        {
            currentSchema.Type = "string";
            currentSchema.Format = "date";
        }

        /// <inheritdoc />
        public void Visit(EnumIdentifierType type)
        {
            currentSchema.Type = "string";
            currentSchema.Enum = type.Values.Cast<object>().ToArray();
        }

        /// <inheritdoc />
        public void Visit(CompositeIdentifierType type)
        {
            var generator = new CompositeMemberGenerator(currentSchema.Properties = new Dictionary<string, Schema>());
            type.Identifiers.Accept(generator);
            currentSchema.Type = "object";
        }

        /// <inheritdoc />
        public void Visit(IntegerIdentifierType type)
        {
            currentSchema.Type = "integer";
        }

        /// <inheritdoc />
        public void Visit(StringIdentifierType type)
        {
            currentSchema.Type = "string";
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition)
        {
            currentSchema = CreateNewSchema(definition);
            
        }

        protected virtual Schema CreateNewSchema(IdentifierDefinition type)
        {
            var valueSchema = new Schema();
            Schemas[type.SystemName] = new Schema
            {
                Type = "object", 
                AllOf = new List<Schema>
                {
                    new Schema { Ref = "#/definitions/IPersonIdentifier" },
                    new Schema
                    {
                        Properties = new Dictionary<string, Schema>
                        {
                            ["value"] = valueSchema
                        }
                    }        
                }
            };
            return valueSchema;
        }

        private class CompositeMemberGenerator : IdentityDefinitionSwaggerSchemaGenerator
        {
            /// <inheritdoc />
            public CompositeMemberGenerator(IDictionary<string, Schema> schemas) : base(schemas)
            {
            }

            /// <inheritdoc />
            protected override Schema CreateNewSchema(IdentifierDefinition type)
            {
                return Schemas[type.SystemName] = new Schema();
            }
        }
    }
}