using System;
using System.Linq;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Tests
{
    public static partial class Create 
    {
        public class IdentifierRegistryBuilder : Builder<PersonIdentifierRegistry, IdentifierRegistryBuilder>
        {
            private Type[] identifierTypes = Array.Empty<Type>();

            public IdentifierRegistryBuilder WithIdentifier<T>() where T : IIdentifier
            {
                return Copy(change: @new => @new.identifierTypes = @new.identifierTypes.Append(typeof(T)).ToArray());
            }

            public IdentifierRegistryBuilder WithIdentifiers<T1, T2>() where T1 : IIdentifier where T2 : IIdentifier
                => WithIdentifier<T1>().WithIdentifier<T2>();

            /// <inheritdoc />
            public override PersonIdentifierRegistry Build()
            {
                var registry = new PersonIdentifierRegistry();
                foreach (var identifierType in identifierTypes)
                {
                    registry.Add(identifierType);
                }

                return registry;
            }
        }

        public static IdentifierRegistryBuilder IdentifierRegistry => new IdentifierRegistryBuilder();
    }
}