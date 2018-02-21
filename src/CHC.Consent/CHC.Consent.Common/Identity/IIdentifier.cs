using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentifier
    {
        [Obsolete]
        Expression<Func<Person, bool>> GetMatchExpression();
        [Obsolete]
        void Update(Person person);
    }
}