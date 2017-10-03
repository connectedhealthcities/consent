using CHC.Consent.Import.Core;

namespace CHC.Consent.Identity.SimpleIdentity
{
    public class SimpleIdentityRecord : IdentityRecord
    {
        public string Value { get; }

        public SimpleIdentityRecord(string identityKindExternalId, string value) : base(identityKindExternalId)
        {
            Value = value;
        }
    }
}