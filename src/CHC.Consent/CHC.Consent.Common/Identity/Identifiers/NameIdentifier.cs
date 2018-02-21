using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class NameIdentifier : IIdentifier
    {
        public string Given { get; set; }
        public string Family { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        /// <inheritdoc />
        public Expression<Func<Person, bool>> GetMatchExpression()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Update(Person person)
        {
            throw new NotImplementedException();
        }
    }
}