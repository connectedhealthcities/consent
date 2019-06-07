using System;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure
{
    public abstract class SerializationBinderDecorator : ISerializationBinder
    {
        private readonly ISerializationBinder innerBinder;

        /// <inheritdoc />
        protected SerializationBinderDecorator(ISerializationBinder innerBinder)
        {
            this.innerBinder = innerBinder;
        }

        /// <inheritdoc />
        public virtual Type BindToType(string assemblyName, string typeName) =>
            innerBinder.BindToType(assemblyName, typeName);

        /// <inheritdoc />
        public virtual void BindToName(Type serializedType, out string assemblyName, out string typeName) =>
            innerBinder.BindToName(serializedType, out assemblyName, out typeName);
    }
}