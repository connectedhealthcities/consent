namespace CHC.Consent.EFCore.Identity
{
    public interface IStringValueParser
    {
        bool TryParse(string value, out object result);
    }
}