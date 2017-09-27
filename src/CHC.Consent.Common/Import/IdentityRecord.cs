namespace CHC.Consent.Common.Import
{
    public abstract class IdentityRecord 
    {
        protected IdentityRecord(string identityKindId)
        {
            IdentityKindId = identityKindId;
        }

        public string IdentityKindId { get; }
    }
}