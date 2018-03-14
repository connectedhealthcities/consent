using System;

namespace CHC.Consent.Common.Consent
{
    public class StudyIdentity : IdentityBase, IEquatable<StudyIdentity>
    {
     
        /// <inheritdoc />
        public StudyIdentity(long id) : base(id)
        {
     
        }

        /// <inheritdoc />
        public bool Equals(StudyIdentity other) => base.Equals(other);
        
        
    }
}