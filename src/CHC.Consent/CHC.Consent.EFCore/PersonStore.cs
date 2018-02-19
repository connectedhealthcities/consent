using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using CHC.Consent.Common;
using CHC.Consent.Common.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CHC.Consent.EFCore
{
    public class PersonStore : IStore<Person>
    {
        private readonly DbSet<PersonEntity> people;
        
        /// <inheritdoc />
        public PersonStore(DbSet<PersonEntity> people)
        {
            this.people = people;
            
        }

        /// <inheritdoc />
        public Type ElementType => typeof(Person);

        /// <inheritdoc />
        public Expression Expression => ((IQueryable) people).Expression;

        /// <inheritdoc />
        public IQueryProvider Provider => ((IQueryable) people).Provider;

        /// <inheritdoc />
        public Person Add(Person value)
        {
            if (value is PersonEntity entity)
            {
                return people.Add(entity).Entity;
            }
            else
            {
                return people.Add(
                    new PersonEntity
                    {
                        Id = value.Id,
                        BirthOrder = value.BirthOrder,
                        DateOfBirth = value.DateOfBirth,
                        NhsNumber = value.NhsNumber,
                        Sex = value.Sex
                    }).Entity;
            }
        }

        /// <inheritdoc />
        public IEnumerator<Person> GetEnumerator() => people.AsQueryable().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
    }
}