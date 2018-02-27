namespace CHC.Consent.Common.Identity
{
    public interface IIdentifier
    {
    }
    
    public interface ISingleValueIdentifier<out T> { T Value { get; }}
}