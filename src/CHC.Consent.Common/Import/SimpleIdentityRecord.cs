namespace CHC.Consent.Common.Import
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