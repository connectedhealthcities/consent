using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.EFCore.Identity;

namespace CHC.Consent.DependencyInjection
{
    public class PersonIdentifierOptions
    {
        public PersonIdentifierOptions(Type type)
        {
            if (!typeof(IPersonIdentifier).IsAssignableFrom(type))
                throw new Exception($"{type} is not {typeof(IPersonIdentifier)}");
            
            IdentifierType = type;
            TypeName = IdentifierAttribute.GetAttribute(type).Name;
        }

        public Type IdentifierType { get; }
        public string TypeName { get; }
        public Func<IServiceProvider, object> HandlerProvider { get; private set; }
        public bool CanHaveDuplicates { get; set; }
        
        public void Validate()
        {
            if(HandlerProvider == null) throw new InvalidOperationException();
        }

        public void SetHandlerFromMarshaller<TIdentifer>(IIdentifierMarshaller<TIdentifer> marshaller) where TIdentifer : IPersonIdentifier
        {
            HandlerProvider = _ => new PersonIdentifierHandler<TIdentifer>(marshaller, TypeName);
        }
    }
}