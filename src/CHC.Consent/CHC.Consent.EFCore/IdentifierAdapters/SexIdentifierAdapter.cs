using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public interface IIdentifierMarshaller<T> where T:IIdentifier
    {
        string ValueType { get; }
        string MarshalledValue(T value);
        T Unmarshall(string valueType, string value);
    }

    public class SexIdentifierMarshaller : IIdentifierMarshaller<SexIdentifier>
    {
        public string ValueType => "sex";

        public string MarshalledValue(SexIdentifier value)
        {
            return value.Sex?.ToString();
        }

        public SexIdentifier Unmarshall(string valueType, string value)
        {
            return valueType == ValueType && Enum.TryParse<Sex>(value, out var sex)  ? new SexIdentifier(sex) : null;
        }
    }

    public class IdentifierAdapterBase<TIdentifier> : 
        IIdentifierFilter<TIdentifier>, IIdentifierRetriever<TIdentifier>, IIdentifierUpdater<TIdentifier> where TIdentifier : IIdentifier
    {
        private readonly IIdentifierMarshaller<TIdentifier> marshaller;
        private readonly string typeName;

        public IdentifierAdapterBase(IIdentifierMarshaller<TIdentifier> marshaller, string typeName)
        {
            this.marshaller = marshaller;
            this.typeName = typeName;
        }

        

        public IQueryable<PersonEntity> Filter(IQueryable<PersonEntity> people,TIdentifier value,IStoreProvider stores)
        {
            var marshalledValue = marshaller.MarshalledValue(value);
            return people.Where(
                p => stores.Get<IdentifierEntity>().Any(
                    _ =>
                        _.Person == p &&
                        _.TypeName == typeName
                        && _.Deleted == null &&
                        _.Value == marshalledValue));
        }

        /// <inheritdoc />
        public IEnumerable<TIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            return  ExistingIdentifierEntities(person, stores)
                .AsEnumerable()
                .Select(_ => marshaller.Unmarshall(_.ValueType, _.Value) );
        }

        private IQueryable<IdentifierEntity> ExistingIdentifierEntities(PersonEntity person, IStoreProvider stores)
        {
            return stores.Get<IdentifierEntity>().Where(_ => _.Person == person && _.TypeName == typeName && _.Deleted == null);
        }

        /// <inheritdoc />
        public bool Update(PersonEntity person, TIdentifier value, IStoreProvider stores)
        {
            var existing = ExistingIdentifierEntities(person, stores).SingleOrDefault();
            if (existing != null && existing.Value != marshaller.MarshalledValue(value))
            {
                existing.Deleted = DateTime.UtcNow;
            }

            stores.Get<IdentifierEntity>().Add(
                new IdentifierEntity
                {
                    Person = person,
                    TypeName = typeName,
                    Value = marshaller.MarshalledValue(value),
                    ValueType = marshaller.ValueType
                });

            return true;
        }
    }

    public class SexIdentifierAdapter : IdentifierAdapterBase<SexIdentifier>
    {
        /// <inheritdoc />
        public SexIdentifierAdapter() : base(new SexIdentifierMarshaller(), SexIdentifier.TypeName)
        {
        }
    }
}