namespace CHC.Consent.Common.Import
{
    public abstract class IdentityRecord 
    {
        protected IdentityRecord(string identityKindExternalId)
        {
            IdentityKindExternalId = identityKindExternalId;
        }

        public string IdentityKindExternalId { get; }
    }
}