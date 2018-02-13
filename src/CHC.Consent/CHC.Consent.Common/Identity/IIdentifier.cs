using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentifier
    {
        Expression<Func<Person, bool>> GetMatchExpression();
        void Update(Person person);
    }
}