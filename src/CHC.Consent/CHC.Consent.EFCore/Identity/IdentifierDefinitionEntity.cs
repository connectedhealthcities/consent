namespace CHC.Consent.EFCore.Identity
{
    public class IdentifierDefinitionEntity : IEntity, IDefinitionEntity 
    {
        /// <inheritdoc />
        protected IdentifierDefinitionEntity()
        {
        }

        /// <inheritdoc />
        public IdentifierDefinitionEntity(string name, string definition)
        {
            Name = name;
            Definition = definition;
        }

        /// <inheritdoc />
        public long Id { get; private set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Definition { get; set; }
    }
}