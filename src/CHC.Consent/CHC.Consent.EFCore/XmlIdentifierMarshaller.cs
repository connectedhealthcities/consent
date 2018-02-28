using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.EFCore.IdentifierAdapters;

namespace CHC.Consent.EFCore
{
    /// <summary>
    /// Naive use of XML serialzier that tries to exclude any namespace declarations, newlines, and xml declarations 
    /// </summary>
    /// <typeparam name="TIdentifier"></typeparam>
    public class XmlIdentifierMarshaller<TIdentifier> : IIdentifierMarshaller<TIdentifier> 
        where TIdentifier : IIdentifier
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(TIdentifier));

        public XmlIdentifierMarshaller(string valueType)
        {
            ValueType = valueType;
        }

        /// <inheritdoc />
        public string ValueType { get; }

        /// <inheritdoc />
        public string MarshalledValue(TIdentifier value)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using (var memoryStream = new MemoryStream())
            {
                var writer = XmlWriter.Create(memoryStream, new XmlWriterSettings{ OmitXmlDeclaration = true, NewLineHandling = NewLineHandling.None});
                
                using (writer)
                {
                    Serializer.Serialize(writer, value, ns);
                }
                memoryStream.Flush();
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        /// <inheritdoc />
        public TIdentifier Unmarshall(string valueType, string value)
        {

            if (valueType != ValueType) throw new ArgumentException($"ValueType '{valueType}' != {ValueType}");
            if (value == null)
                throw new ArgumentNullException(
                    nameof(value),
                    $"Cannot make a {nameof(MedwayNameIdentifier)} from null");
            
            return (TIdentifier) Serializer.Deserialize(
                new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(value)), Encoding.UTF8));
        }
    }
}