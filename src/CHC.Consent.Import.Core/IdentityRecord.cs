namespace CHC.Consent.Import.Core
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