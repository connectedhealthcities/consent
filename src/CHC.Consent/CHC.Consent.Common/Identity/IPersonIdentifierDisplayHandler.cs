namespace CHC.Consent.Common.Identity
{
    public interface IPersonIdentifierDisplayHandler
    {
        string DisplayName { get; }
    }
    
    public interface IPersonIdentifierDisplayHandler<T> : IPersonIdentifierDisplayHandler where T : IPersonIdentifier
    {
    }
}