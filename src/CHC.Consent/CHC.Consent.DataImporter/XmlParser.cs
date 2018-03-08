using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using CHC.Consent.Api.Client.Models;
using Newtonsoft.Json;

namespace CHC.Consent.DataImporter
{
    public class XmlParser
    {
        public IEnumerable<PersonSpecification> GetPeople(StreamReader source)
        {
            var xmlReader = XmlReader.Create(source);

            return GetPeople(xmlReader);
        }

        public IEnumerable<PersonSpecification> GetPeople(XmlReader xmlReader)
        {
            var personIdentifierTypes = typeof(PersonSpecification).Assembly.GetExportedTypes()
                .Where(type => type.IsSubclassOf(typeof(IPersonIdentifier)))
                .ToDictionary(type => type.GetCustomAttribute<JsonObjectAttribute>().Id);

            xmlReader = XmlReader.Create(xmlReader, new XmlReaderSettings{ IgnoreWhitespace = true});

            xmlReader.MoveToContent();

            if (xmlReader.LocalName != "people")
            {
                //TODO: Record errors
                throw new NotImplementedException();
            }

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.LocalName != "person")
                {
                    //TODO: record errors
                    continue;
                }


                var personNode = XDocument.Load(xmlReader.ReadSubtree(), LoadOptions.SetLineInfo);

                var person = new PersonSpecification
                {
                    Identifiers = personNode.XPathSelectElements("/person/identity/identifier")
                        .Select(_ =>
                        {
                            try
                            {
                                return ParseIdentifier(_, personIdentifierTypes);
                            }
                            catch (Exception e)
                            {
                             
                                throw;
                            }
                        })
                        .ToList()
                };

                yield return person;
            }
        }

        public static IPersonIdentifier ParseIdentifier(XElement identifierNode, Dictionary<string, Type> typeNameLookup)
        {
            var typeName = identifierNode.Attribute("type")?.Value;
            if (string.IsNullOrEmpty(typeName))
            {
                //TODO: Error recording
                return null;
            }

            if (!typeNameLookup.ContainsKey(typeName))
            {
                //TODO: error recording
                return null;
            }

            var identifierType = typeNameLookup[typeName];

            XmlAttributeOverrides overrides = new XmlAttributeOverrides();


            var onlyProperty = identifierType.GetProperties()
                .Where(_ => _.GetCustomAttribute<JsonPropertyAttribute>() != null).ToArray();
            var constructor = identifierType.GetConstructors()
                .Select(_ => new {Constructor = _, Parameters = _.GetParameters()})
                .OrderByDescending(_ => _.Parameters.Length)
                .First();


            var nonTypeAttributes = identifierNode.Attributes().Where(_ => _.Name.LocalName != "type");
            var attributeValues = nonTypeAttributes.Any()
                ? nonTypeAttributes.Select(_ => new {Name = _.Name.LocalName, _.Value})
                : Enumerable.Repeat(new {Name = (string) null, Value = (string) null}, 0);

            if (identifierNode.FirstNode.NodeType == XmlNodeType.Text)
            {
                var node = identifierNode.FirstNode;
                var type = constructor.Parameters[0].ParameterType;
                var stringValue = ((XText) node).Value;

                var typedValue = ConvertStringToCorrectType(stringValue, type);
                return (IPersonIdentifier) constructor.Constructor.Invoke(new[] {typedValue});
            }

            var parameterValues = new List<object>();
            foreach (var parameter in constructor.Parameters)
            {
                //TODO: add some checking here
                var parameterElement = identifierNode.Element(parameter.Name);

                if (parameterElement == null)
                {   
                    if (parameter.HasDefaultValue)
                    {
                        parameterValues.Add(parameter.DefaultValue);
                    }
                    else
                    {
                        //TODO: Log this and handle more gracefully
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    var value = (parameterElement.FirstNode as XText)?.Value;
                    parameterValues.Add(ConvertStringToCorrectType(value, parameter.ParameterType));
                }
            }

            return (IPersonIdentifier) constructor.Constructor.Invoke(parameterValues.ToArray());
        }

        private static object ConvertStringToCorrectType(string stringValue, Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type);
                if (string.IsNullOrEmpty(stringValue)) return null;
            }

            var typedValue = Convert.ChangeType(stringValue, type);
            return typedValue;
        }
    }
}