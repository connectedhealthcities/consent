namespace CHC.Consent.Common.Identity
{
    public interface IIdentifierValue<out T>
    {
        T Value { get; }
    }
}