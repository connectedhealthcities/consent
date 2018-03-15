using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CHC.Consent.Common.Identity.Identifiers.Medway;

namespace CHC.Consent.EFCore
{

    /// <summary>
    /// Helper for mangling <see cref="MarshalledType"/> to/from <see cref="string"/>
    /// </summary>
    /// <remarks>
    /// <para>implements Flyweight pattern for XmlSerializer</para>
    /// </remarks>
    class XmlMarshaller
    {
        public Type MarshalledType { get; }
        public string ValueType { get; }

        private static readonly ConcurrentDictionary<Type, XmlSerializer> Serializers =
            new ConcurrentDictionary<Type, XmlSerializer>();

        private static XmlSerializer GetSerializer(Type type) =>
            Serializers.GetOrAdd(type, _ => new XmlSerializer(type));

        /// <param name="marshalledType">*Must* be compatilble with <see cref="XmlSerializer"/></param>
        public XmlMarshaller(Type marshalledType, string valueType)
        {
            //this is a cheap way to check that the marhsalled type can be used with xmlserialization without
            //waiting until the serializer is used
            GetSerializer(marshalledType);
            MarshalledType = marshalledType;
            ValueType = valueType;
        }

        private XmlSerializer Serializer => GetSerializer(MarshalledType);

        public string MarshalledValue(object value)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using (var memoryStream = new MemoryStream())
            {
                var writer = XmlWriter.Create(
                    memoryStream,
                    new XmlWriterSettings {OmitXmlDeclaration = true, NewLineHandling = NewLineHandling.None});
                
                using (writer)
                {
                    Serializer.Serialize(writer, value, ns);
                }
                memoryStream.Flush();
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public object Unmarshall(string valueType, string value)
        {

            if (valueType != ValueType) throw new ArgumentException($"ValueType '{valueType}' != {ValueType}");
            if (value == null)
                throw new ArgumentNullException(
                    nameof(value),
                    $"Cannot make a {MarshalledType.Name} from null");
            
            return Serializer.Deserialize(
                new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(value)), Encoding.UTF8));
        }
    }
    
    /// <summary>
    /// Performs xml serialization and deserialization for <typeparamref name="T"/>
    /// </summary>
    /// <remarks><typeparamref name="T"/> must be compatible with <see cref="XmlSerializer"/></remarks>
    public class XmlMarshaller<T>
    {
        public XmlMarshaller(string valueType)
        {
            Marshaller = new XmlMarshaller(typeof(T), valueType);    
        }

        private XmlMarshaller Marshaller { get;  }

        public string ValueType => Marshaller.ValueType;

        /// <inheritdoc />
        public string MarshalledValue(T value) => Marshaller.MarshalledValue(value);

        /// <inheritdoc />
        public T Unmarshall(string valueType, string value) => (T) Marshaller.Unmarshall(valueType, value);
    }
}