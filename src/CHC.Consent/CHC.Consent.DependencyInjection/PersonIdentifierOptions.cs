using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
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
            
            InitialiseDisplayHandlerProvider(type, attribute);
        }

        private void InitialiseDisplayHandlerProvider(Type type, IdentifierAttribute attribute)
        {
            if (attribute.DisplayName == null) return;
            var displayHandlerProvider = Activator.CreateInstance(
                typeof(IdentifierAttributePersonIdentifierDisplayHandler<>).MakeGenericType(type));
            DisplayHandlerProvider = x => displayHandlerProvider;
        }

        public Type IdentifierType { get; }
        public string TypeName { get; }
        public Func<IServiceProvider, object> PersistanceHandlerProvider { get; private set; }
        public bool CanHaveDuplicates { get; set; }
        public Func<IServiceProvider, object> DisplayHandlerProvider { get; private set; }

        public void Validate()
        {
            if(PersistanceHandlerProvider == null) throw new InvalidOperationException();
        }

        public void SetHandlerFromMarshaller<TIdentifer>(IIdentifierMarshaller<TIdentifer> marshaller) where TIdentifer : IPersonIdentifier
        {
            PersistanceHandlerProvider = _ => 
                new PersonIdentifierPersistanceHandler<TIdentifer>(
                marshaller,
                TypeName,
                _.GetRequiredService<ILogger<PersonIdentifierPersistanceHandler<TIdentifer>>>());
        }
    }
}