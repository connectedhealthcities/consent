using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using CHC.Consent.Api.Client.Models;
using Newtonsoft.Json;

namespace CHC.Consent.DataImporter
{
    /// <summary>
    /// <para>Simple XML parser that tries to match element names against constructor arguments.</para>
    /// <para>This will always find the constructor with most arguments and try to apply that.</para>
    /// <para>It honours default argument values. In the case of only one argument, the contents of the node is used</para>
    /// </summary>
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

            xmlReader = XmlReader.Create(
                xmlReader,
                new XmlReaderSettings {IgnoreWhitespace = true, IgnoreComments = true,});

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


                var personNode = XDocument.Load(xmlReader.ReadSubtree(), LoadOptions.SetLineInfo|LoadOptions.SetBaseUri);

                var person = new PersonSpecification
                {
                    Identifiers = personNode.XPathSelectElements("/person/identity/*")
                        .Select(_ => ParseIdentifier(_, personIdentifierTypes))
                        .ToList(),
                    MatchSpecifications =
                        personNode.XPathSelectElements("/person/lookup/match")
                            .Select(
                                m => new MatchSpecification(
                                    m.Elements()
                                        .Select(_ => ParseIdentifier(_, personIdentifierTypes))
                                        .ToList()))
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
                var nodeName = identifierNode.Name;
                var prefix = string.IsNullOrEmpty(nodeName.NamespaceName) ? string.Empty : nodeName.NamespaceName + ".";
                typeName = prefix + KebabCase(nodeName.LocalName);
                //return null;
            }

            if (!typeNameLookup.ContainsKey(typeName))
            {
                throw new XmlParseException(identifierNode, $"Cannot find type '{typeName}'");
            }

            var identifierType = typeNameLookup[typeName];

            var constructor = identifierType.GetConstructors()
                .Select(_ => new {Constructor = _, Parameters = _.GetParameters()})
                .OrderByDescending(_ => _.Parameters.Length)
                .First();


            
            if (identifierNode.IsEmpty || identifierNode.FirstNode.NodeType == XmlNodeType.Text)
            {
                var node = identifierNode.FirstNode;
                var stringValue = ((XText) node)?.Value;
                var type = constructor.Parameters[0].ParameterType;

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
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType == null)
            {
                return ChangeType(stringValue, type);
            }

            if (string.IsNullOrEmpty(stringValue)) return null;

            return ChangeType(stringValue, underlyingType);
        }

        private static object ChangeType(string stringValue, Type type) => 
            type.IsEnum
            ? Enum.Parse(type, stringValue)
            : Convert.ChangeType(stringValue, type);

        private static readonly Regex lowerCaseFollowedByUpperCase = new Regex(
            @"(\p{Ll})(\p{Lu})",
            RegexOptions.Compiled);
        private static string KebabCase(string name)
        {
            return lowerCaseFollowedByUpperCase.Replace(name, match => match.Groups[1] + "-" + match.Groups[2].Value.ToLowerInvariant());
        }
    }
}