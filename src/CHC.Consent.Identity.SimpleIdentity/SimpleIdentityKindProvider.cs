using System;
using System.Linq.Expressions;
using System.Xml.Linq;
using CHC.Consent.Identity.Core;
using CHC.Consent.Import.Core;
using CHC.Consent.NHibernate.Identity;

namespace CHC.Consent.Identity.SimpleIdentity
{
    public class SimpleIdentityKindProvider : IIdentityKindHelper
    {
        public IdentityRecord ReadXml(string identityKindExternalId, XElement element)
        {
            return new SimpleIdentityRecord(
                identityKindExternalId,
                element.Element(XmlNames.Value).Value);
        }

        public IIdentity ConvertToIdentity(Guid identityKindId, IdentityRecord record)
        {
            if (!(record is SimpleIdentityRecord simple))
                throw new ArgumentException($"Cannot convert identity of type {record.GetType()}", nameof(record));
            
            return new SimpleIdentitySpecification(identityKindId, simple.Value);
        }

        public Expression<Func<NHibernate.Identity.Identity, bool>> CreateMatchQuery(IIdentity match)
        {
            if(!(match is ISimpleIdentity simpleIdentity))
            {
                throw new ArgumentException($"Cannot create query for identity of type {match.GetType()}", nameof(match));
            }
            
            return identity =>
                identity.IdentityKindId == simpleIdentity.IdentityKindId &&
                identity is SimpleIdentity &&
                ((SimpleIdentity) identity).Value == simpleIdentity.Value;
        }

        public NHibernate.Identity.Identity CreatePersistedIdentity(IIdentity identity)
        {
            if (identity is IPersistedSimpleIdentitySource source)
            {
                return source.CreatePersistedIdentity();
            }
            
            throw new ArgumentException($"Cannot create a persisted identity from {identity}");
        }
    }    
}