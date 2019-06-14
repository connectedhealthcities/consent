namespace CHC.Consent.EFCore
{
    public interface IIdentifierEntity 
    {
        string TypeName { get; }
        string Value { get; }
    }
}