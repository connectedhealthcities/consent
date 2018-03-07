using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Consent
{
    public abstract class ConsentIdentifier
    {
        public abstract void Update(Consent consent);
        public abstract Expression<Func<Consent,bool>> CreateMatchIdentifier();
    }
}