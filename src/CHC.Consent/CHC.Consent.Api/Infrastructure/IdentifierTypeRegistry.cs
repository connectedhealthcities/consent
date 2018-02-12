using System.Collections;
using System.Collections.Generic;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Infrastructure
{
    public class IdentifierTypeRegistry : IIdentifierTypeRegistry, IEnumerable<IdentifierType>
    {
        private Dictionary<string, IdentifierType> NamesToTypes { get; } = new Dictionary<string, IdentifierType>();
        
        public void Add<TIdentifierType>() 
            where TIdentifierType: IdentifierType, new()
        {
            Add(new TIdentifierType());
        }

        public void Add<TIdentifierType>(TIdentifierType identifierType)
            where TIdentifierType : IdentifierType
        {
            NamesToTypes.Add(identifierType.ExternalId, identifierType);
        }

        public IdentifierType GetIdentifierType(string externalId)
        {
            return NamesToTypes.TryGetValue(externalId, out var type) ? type : null;
        }

        public IEnumerator<IdentifierType> GetEnumerator() => NamesToTypes.Values.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}