using CHC.Consent.Common.Identity;

namespace CHC.Consent.EFCore.Identity
{
    public interface IIdentifierMarshaller<T> where T:IPersonIdentifier
    {
        string ValueType { get; }
        string MarshalledValue(T value);
        T Unmarshall(string valueType, string value);
    }
}