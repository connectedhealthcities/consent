using CHC.Consent.Common.Identity;

namespace CHC.Consent.EFCore.Identity
{
    /// <summary>
    /// Naive use of XML serialzier that tries to exclude any namespace declarations, newlines, and xml declarations 
    /// </summary>
    /// <typeparam name="TIdentifier"></typeparam>
    public class XmlIdentifierMarshaller<TIdentifier> : XmlMarshaller<TIdentifier>, IIdentifierMarshaller<TIdentifier> 
        where TIdentifier : IPersonIdentifier
    {
        public XmlIdentifierMarshaller(string valueType) : base(valueType)
        {
        }
    }
}