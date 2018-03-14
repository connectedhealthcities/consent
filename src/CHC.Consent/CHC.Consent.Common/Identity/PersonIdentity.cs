using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Common
{
    public class PersonIdentity : IdentityBase 
    {
        /// <inheritdoc />
        public PersonIdentity(long id) : base(id)
        {
        }
    }
}