using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.SqlServer
{
    public class PersonStore : IStore<Person>
    {
        /// <inheritdoc />
        public Person Add(Person value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerator<Person> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public Type ElementType { get; set; }

        /// <inheritdoc />
        public Expression Expression { get; set; }

        /// <inheritdoc />
        public IQueryProvider Provider { get; set; }
    }
}