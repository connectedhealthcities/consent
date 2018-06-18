using System.Reflection;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IdentifierAttributePersonIdentifierDisplayHandler<TIdentifier> : 
        IPersonIdentifierDisplayHandler<TIdentifier> 
        where TIdentifier : IPersonIdentifier
    {
        private static readonly string displayName; 
        static IdentifierAttributePersonIdentifierDisplayHandler()
        {
            displayName = typeof(TIdentifier).GetCustomAttribute<IdentifierAttribute>().DisplayName;
        }
        /// <inheritdoc />
        public string DisplayName => displayName;
    }
}