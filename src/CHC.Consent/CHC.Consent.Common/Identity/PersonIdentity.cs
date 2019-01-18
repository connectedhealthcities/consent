using System;
using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Common
{
    public class PersonIdentity : IdentityBase 
    {
        /// <inheritdoc />
        public PersonIdentity(long id) : base(id)
        {
        }
        
        public static explicit operator PersonIdentity(long id) => new PersonIdentity(id);
    }
}