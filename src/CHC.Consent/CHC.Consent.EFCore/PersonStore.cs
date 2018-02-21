using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using CHC.Consent.Common;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace CHC.Consent.EFCore
{
    public class PersonStore : IStore<Person>
    {
        private readonly DbSet<PersonEntity> people;
        private readonly IQueryable<PersonEntity> queryable;

        /// <inheritdoc />
        public PersonStore(DbSet<PersonEntity> people)
        {
            this.people = people;
            queryable = people.Include(_ => _.BradfordHospitalNumberEntities);
        }

        /// <inheritdoc />
        public Type ElementType => typeof(Person);

        /// <inheritdoc />
        public Expression Expression => queryable.Expression;

        /// <inheritdoc />
        public IQueryProvider Provider => queryable.Provider;

        /// <inheritdoc />
        public Person Add(Person value)
        {
            if (value is PersonEntity entity)
            {
                return people.Add(entity).Entity;
            }
            else
            {
                var newEntity = new PersonEntity
                {
                    Id = value.Id,
                    BirthOrder = value.BirthOrder,
                    DateOfBirth = value.DateOfBirth,
                    NhsNumber = value.NhsNumber,
                    Sex = value.Sex
                };
                foreach (var hospitalNumber in value.BradfordHospitalNumbers)
                {
                    newEntity.AddHospitalNumber(hospitalNumber);
                }
                return people.Add(newEntity).Entity;
            }
        }

        /// <inheritdoc />
        public Person Get(long id)
        {
            return people.Find(id);
        }

        /// <inheritdoc />
        public IEnumerator<Person> GetEnumerator() => people.AsQueryable().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
    }
}