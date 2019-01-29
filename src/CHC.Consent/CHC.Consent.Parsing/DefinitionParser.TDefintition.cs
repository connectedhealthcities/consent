using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;
using Sprache;

namespace CHC.Consent.Parsing
{
    /// <summary>
    /// <para>Parses definitions from a string of form: </para>
    /// <code> name : type </code>
    /// <code> name : enum ( 'value', 'value2', ... ) </code>
    /// <code> name : composite ( field: type, ... ) </code>
    /// </summary>
    /// <remarks>
    /// <para><c>name</c> should be a character (a-z, 0-9), followed by hyphen or character, ending with a character)</para>
    /// <para><c>type</c> is currently: string, date, integer, enum, composite</para>
    /// <para><c>enum</c> is <c>enum('value',...)</c> - values are wrapped in single quotes</para>
    /// <para><c>composite</c> is <c>composite( name:type, ... )</c> - these can be recursive</para> 
    /// </remarks>
    public class DefinitionParser<TDefinition> where TDefinition:IDefinition
    {
        public delegate TDefinition DefinitionCreator(string name, IDefinitionType type);

        private DefinitionCreator CreateDefinition { get; }

        public DefinitionParser(DefinitionCreator createDefinition)
        {
            CreateDefinition = createDefinition;
        }

        private Parser<IDefinitionType> Composite =>
            from _ in Parse.String("composite").Text()
            from parts in Parse.Ref(
                () => DefinitionList.Contained(DefinitionParser.OpenBracket, DefinitionParser.CloseBracket)
            ).Named("a Definition List").Token()
            select new CompositeDefinitionType(parts.Cast<IDefinition>().ToArray());

        private Parser<TDefinition> Definition => 
            from name in DefinitionParser.Identifier.Named("a Name").Token()
            from _ in Parse.Char(':').Named("a Colon ':'")
            from type in TypeParser
            select CreateDefinition(name, type);

        private Parser<IDefinitionType> TypeParser =>
            DefinitionParser.SimpleTypes
                .XOr(DefinitionParser.EnumType)
                .XOr(Composite)
                .Named("a Type (string, date, etc...)")
                .Token();

        private Parser<IEnumerable<TDefinition>> DefinitionList => 
            Definition.Token().XDelimitedBy(DefinitionParser.Comma);

        public TDefinition ParseString(string definition)
        {
            return Definition.Parse(definition);
        }
    }
}