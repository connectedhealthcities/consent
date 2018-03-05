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
        public Func<IServiceProvider, object> FilterProvider { get; set; }
        public Func<IServiceProvider, object> UpdaterProvider { get; set; }
        public Func<IServiceProvider, object> RetrieverProvider { get; set; }
        public bool CanHaveDuplicates { get; set; }
        
        public void Validate()
        {
            if(FilterProvider == null) throw new InvalidOperationException();
            if(UpdaterProvider == null) throw new InvalidOperationException();
            if(RetrieverProvider == null) throw new InvalidOperationException();
        }

        public void SetHandlerFromMarshaller<TIdentifer>(IIdentifierMarshaller<TIdentifer> identifierMarshaller) where TIdentifer : IPersonIdentifier
        {
            var handler = new PersonIdentifierAdapter<TIdentifer>(identifierMarshaller, TypeName);

            FilterProvider = _ => handler;
            RetrieverProvider = _ => handler;
            UpdaterProvider = _ => handler;
        }
    }
}