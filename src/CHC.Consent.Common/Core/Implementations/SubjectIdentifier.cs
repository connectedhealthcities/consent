using System;

namespace CHC.Consent.Common.Core
{
    public class SubjectIdentifier : ISubjectIdentifier
    {
        public IStudy Study { get; }

        public Guid StudyId => Study.Id;
        public string Identifier { get; }

        /// <summary>
        /// For EF
        /// </summary>
        protected SubjectIdentifier()
        {
        }

        public SubjectIdentifier(IStudy study, string identifier)
        {
            Study = study;
            Identifier = identifier;
        }
    }
}