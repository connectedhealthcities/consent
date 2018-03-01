using CHC.Consent.Common.Identity;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.IdentifierAdapters;

namespace CHC.Consent.DependencyInjection
{
    public class PersonIdentifierOptionsBuilder<TIdentifer> where TIdentifer : IIdentifier
    {
        public PersonIdentifierOptions Options { get; }

        public PersonIdentifierOptionsBuilder(PersonIdentifierOptions options)
        {
            Options = options;
        }


        public PersonIdentifierOptionsBuilder<TIdentifer> WithMarshaller<TMarshaller>() where TMarshaller:IIdentifierMarshaller<TIdentifer>, new()
        {
            Options.SetHandlerFromMarshaller<TIdentifer>(new TMarshaller());
            return this;
        }

        public PersonIdentifierOptionsBuilder<TIdentifer> WithXmlMarshaller(string valueType)
        {
            Options.SetHandlerFromMarshaller<TIdentifer>(new XmlIdentifierMarshaller<TIdentifer>(valueType));
            return this;
        }

        public PersonIdentifierOptionsBuilder<TIdentifer> CanHaveMultipleValuesSpecified()
        {
            Options.CanHaveDuplicates = true;
            return this;
        }
    }
}