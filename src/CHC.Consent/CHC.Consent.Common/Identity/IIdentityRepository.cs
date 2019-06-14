using System.Collections.Generic;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentityRepository
    {
        PersonIdentity FindPersonBy(IPersonSpecification specification);
        IEnumerable<PersonIdentifier> GetPersonIdentifiers(long personId);
        PersonIdentity CreatePerson(IEnumerable<PersonIdentifier> identifiers, Authority authority);

        void UpdatePerson(
            PersonIdentity personIdentity, IEnumerable<PersonIdentifier> identifiers, Authority authority);

        IDictionary<PersonIdentity, IEnumerable<PersonIdentifier>> GetPeopleWithIdentifiers(
            IEnumerable<PersonIdentity> personIds,
            IEnumerable<string> identifierNames,
            IUserProvider user);

        IDictionary<PersonIdentity, IEnumerable<PersonIdentifier>> GetPeopleWithIdentifiers(
            IEnumerable<PersonIdentity> personIds,
            IEnumerable<string> identifierNames,
            IUserProvider user,
            IEnumerable<IdentifierSearch> search, 
            string subjectIdentifier);

        Authority GetAuthority(string systemName);
        Agency GetAgency(string systemName);
        string GetPersonAgencyId(PersonIdentity personId, AgencyIdentity agencyId);
        Study GetStudy(StudyIdentity studyId);
    }

    public interface IPersonSpecification
    {
        void Accept(IPersonSpecificationVisitor visitor);
    }

    public interface IPersonSpecificationVisitor
    {
        void Visit(PersonIdentifierSpecification specification);
        void Visit(AgencyIdentifierPersonSpecification specification);
        void Visit(ConsentedPersonSpecification specification);
    }


    public class CompositePersonSpecification : IPersonSpecification
    {
        /// <inheritdoc />
        public CompositePersonSpecification(IEnumerable<IPersonSpecification> specifications)
        {
            Specifications = specifications;
        }

        public CompositePersonSpecification(params IPersonSpecification[] specifications)
        {
            Specifications = specifications;
        }

        public IEnumerable<IPersonSpecification> Specifications { get; }


        /// <inheritdoc />
        public void Accept(IPersonSpecificationVisitor visitor)
        {
            foreach (var specification in Specifications)
            {
                specification.Accept(visitor);
            }
        }
    }

    public class PersonIdentifierSpecification : IPersonSpecification
    {
        /// <inheritdoc />
        public PersonIdentifierSpecification(PersonIdentifier personIdentifier)
        {
            PersonIdentifier = personIdentifier;
        }

        public PersonIdentifier PersonIdentifier { get; }

        public void Accept(IPersonSpecificationVisitor visitor) => visitor.Visit(this);

        public static implicit operator PersonIdentifierSpecification(PersonIdentifier personIdentifier)
            => new PersonIdentifierSpecification(personIdentifier);
    }

    public class AgencyIdentifierPersonSpecification : IPersonSpecification
    {
        /// <inheritdoc />
        public AgencyIdentifierPersonSpecification(string agencyName, string personAgencyId)
        {
            AgencyName = agencyName;
            PersonAgencyId = personAgencyId;
        }

        public string AgencyName { get; }
        public string PersonAgencyId { get; }
        public void Accept(IPersonSpecificationVisitor visitor) => visitor.Visit(this);
    }

    public class ConsentedPersonSpecification : IPersonSpecification
    {
        /// <inheritdoc />
        public ConsentedPersonSpecification(StudyIdentity studyId)
        {
            StudyId = studyId;
        }

        public StudyIdentity StudyId { get; }

        public void Accept(IPersonSpecificationVisitor visitor) => visitor.Visit(this);
    }
}