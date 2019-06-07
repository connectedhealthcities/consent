using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CHC.Consent.Common.Infrastructure;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api.Infrastructure
{
    public class DerivedTypeSerializationBinder<TBaseType> : SerializationBinderDecorator
    {
        private readonly Lazy<IDictionary<string, Type>> typesByNameLazy;
        private readonly Lazy<IDictionary<Type, string>> namesByTypeLazy;

        private IDictionary<string, Type> TypesByName => typesByNameLazy.Value;
        private IDictionary<Type, string> NamesByType => namesByTypeLazy.Value;

        /// <inheritdoc />
        public DerivedTypeSerializationBinder(ISerializationBinder innerBinder) : base(innerBinder)
        {
            typesByNameLazy = new Lazy<IDictionary<string, Type>>(GetTypesByName);
            namesByTypeLazy =
                new Lazy<IDictionary<Type, string>>(() => TypesByName.ToDictionary(_ => _.Value, _ => _.Key));
        }

        private static Dictionary<string, Type> GetTypesByName()
        {
            
            var entryAssembly = AppDomain.CurrentDomain.GetAssemblies();
            return entryAssembly
                .SelectMany(a => a.GetTypes().Where(t => t.IsSubtypeOf(typeof(TBaseType))))
                .Where(t => t.IsConcreteType())
                .ToDictionary(t => t.FriendlyId());
        }

        /// <inheritdoc />
        public override Type BindToType(string assemblyName, string typeName) =>
            TypesByName.TryGetValue(typeName, out var type)
                ? type
                : base.BindToType(assemblyName, typeName);

        /// <inheritdoc />
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            if (!NamesByType.TryGetValue(serializedType, out typeName))
            {
                typeName = serializedType.FriendlyId();
            }
        }
    }
}