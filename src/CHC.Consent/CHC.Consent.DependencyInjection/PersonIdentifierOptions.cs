using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.EFCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CHC.Consent.DependencyInjection
{
    public class PersonIdentifierOptions
    {
        public PersonIdentifierOptions(Type type)
        {
            if (!typeof(IPersonIdentifier).IsAssignableFrom(type))
                throw new Exception($"{type} is not {typeof(IPersonIdentifier)}");
            
            IdentifierType = type;
            var attribute = IdentifierAttribute.GetAttribute(type);
            TypeName = attribute.Name;
            CanHaveDuplicates = attribute.AllowMultipleValues;
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
            HandlerProvider = _ => 
                new PersonIdentifierHandler<TIdentifer>(
                marshaller,
                TypeName,
                _.GetRequiredService<ILogger<PersonIdentifierHandler<TIdentifer>>>());
        }
    }
}