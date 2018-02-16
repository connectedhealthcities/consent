using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Consent
{
    public class ConsentTypeRegistry : ITypeRegistry
    {
        private readonly ConsentIdentifierRegistry identifiers;
        private readonly EvidenceRegistry evidence;

        public ConsentTypeRegistry(ConsentIdentifierRegistry identifiers, EvidenceRegistry evidence)
        {
            this.identifiers = identifiers;
            this.evidence = evidence;
        }

        private IEnumerable<ITypeRegistry> Registries
        {
            get
            {
                yield return identifiers;
                yield return evidence;
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<ClassMapping> GetEnumerator() => Registries.SelectMany(_ => _).GetEnumerator();

        /// <inheritdoc />
        public string GetName(Type type) => Registries.Select(_ => _.GetName(type)).FirstOrDefault(_ => _ != null);

        /// <inheritdoc />
        public Type GetType(string name) => Registries.Select(_ => _.GetType(name)).FirstOrDefault(_ => _ != null);
        

        /// <inheritdoc />
        public Type this[string name] =>
            GetType(name) ?? throw new KeyNotFoundException($"Cannot find type named '{name}'");

        /// <inheritdoc />
        public string this[Type type] =>
            GetName(type) ?? throw new KeyNotFoundException($"Cannot find name for type '{type}'");
    }
}