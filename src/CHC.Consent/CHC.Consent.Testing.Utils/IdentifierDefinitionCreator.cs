using System.IO;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;

namespace CHC.Consent.Testing.Utils
{
    public class IdentifierDefinitionCreator : IDefinitionVisitor
    {
        private readonly StringWriter text = new StringWriter();
        public string DefinitionText => text.ToString();

        /// <inheritdoc />
        public void Visit(IDefinition definition, DateDefinitionType type) => Write(definition, type);

        private void Write(IDefinition definition, IDefinitionType type)
        {
            text.Write("{0}:{1}", definition.SystemName, type.SystemName);
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, EnumDefinitionType type) => Write(definition, type);

        /// <inheritdoc />
        public void Visit(IDefinition definition, CompositeDefinitionType type)
        {
            Write(definition, type);
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, IntegerDefinitionType type) => Write(definition, type);

        /// <inheritdoc />
        public void Visit(IDefinition definition, StringDefinitionType type) => Write(definition, type);
    }
}