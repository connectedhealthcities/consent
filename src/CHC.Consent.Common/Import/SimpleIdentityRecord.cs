namespace CHC.Consent.Common.Import
{
    public class SimpleIdentityRecord : IdentityRecord
    {
        public string Value { get; }

        public SimpleIdentityRecord(string identityKindId, string value) : base(identityKindId)
        {
            Value = value;
        }
    }
}