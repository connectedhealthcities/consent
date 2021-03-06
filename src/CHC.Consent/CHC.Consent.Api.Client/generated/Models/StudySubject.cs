// <auto-generated>
// (C) 2018 CHC  License: TBC
// </auto-generated>

namespace CHC.Consent.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class StudySubject
    {
        /// <summary>
        /// Initializes a new instance of the StudySubject class.
        /// </summary>
        public StudySubject()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the StudySubject class.
        /// </summary>
        public StudySubject(string subjectIdentifier = default(string), StudyIdentity studyId = default(StudyIdentity), PersonIdentity personId = default(PersonIdentity))
        {
            SubjectIdentifier = subjectIdentifier;
            StudyId = studyId;
            PersonId = personId;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "subjectIdentifier")]
        public string SubjectIdentifier { get; private set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "studyId")]
        public StudyIdentity StudyId { get; private set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "personId")]
        public PersonIdentity PersonId { get; private set; }

    }
}
