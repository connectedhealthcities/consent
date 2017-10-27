using System;
using System.Linq;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Consent;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.NHibernate.Tests;
using CHC.Consent.Security;
using CHC.Consent.Testing.NHibernate;
using CHC.Consent.Utils;
using CHC.Consent.WebApi.Abstractions;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.NHibernate.WebApi.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class SecurePersonRepositoryTests
    {
        private User user;
        private Person person;
        private readonly Permisson readPermisson = new Permisson {Name = "read"};
        private readonly PersonRepository personRepository;
        public DatabaseFixture Db { get; }

        /// <inheritdoc />
        public SecurePersonRepositoryTests(DatabaseFixture db, ITestOutputHelper output)
        {
            Db = db;
            Db.StartSession(output.WriteLine).Dispose();
            
            LoggerProvider.SetLoggersFactory(new OutputLoggerFactory(output));
            user = new User();
            person = new Person(Enumerable.Empty<Identity.Identity>());

            Db.InTransactionalUnitOfWork(SavePeople);
            personRepository = new PersonRepository(new UserAccessor(() => user), Db.SessionAccessor);
        }

        public class Paging : IPagingProperties{
            /// <inheritdoc />
            public int Page { get; set; }

            /// <inheritdoc />
            public int PageSize { get; set; }
        }

        public class PersonProjection
        {
            public Guid Id { get; set; }
        }

        [Fact]
        public void HasNotAccessToPersonByDefault()
        {
            Assert.Empty(GetAccessiblePeople());
        }

        [Fact]
        public void HasAccessToPeopleViaExplicitAccess()
        {
            Db.InTransactionalUnitOfWork(AddReadAccessForUser);

            var people = GetAccessiblePeople();
            
            Assert.NotEmpty(people);
        }

        private IPerson[] GetAccessiblePeople()
        {
            return Db.InTransactionalUnitOfWork(
                () => personRepository
                    .GetPeople()
                    .ToArray());
        }

        [Fact]
        public void CanProjectPeople()
        {
            Db.InTransactionalUnitOfWork(AddReadAccessForUser);

            var people = Db.InTransactionalUnitOfWork(
                () => personRepository
                    .GetPeople()
                    .Skip(0).Take(10)
                    .Select(p => new PersonProjection{ Id = p.Id})
                    .ToArray());
            
            Assert.NotEmpty(people);
        }

        [Fact]
        public void CanAccessPersonViaRole()
        {
            var role = new Role { Name = "access role" };

            void AddRoleAccessToPerson(ISession session)
            {
                session.Save(role);
                user = session.Merge(user);
                user.Role = role;
                
                person = session.Merge(person);
                
                person.Acl.Entries.Add(
                    new AccessControlEntry
                    {
                        AccessControlList = person.Acl,
                        Permission = readPermisson,
                        Principal = role
                    });
            }

            Db.InTransactionalUnitOfWork(AddRoleAccessToPerson);

            Assert.NotEmpty(GetAccessiblePeople());

        }

        [Fact]
        public void AccessToStudyDoesNotImplyAccessToPerson()
        {
            var study = new Study();

            Action<ISession> GiveUserAccessToStudy = s =>
            {
                s.Merge(readPermisson);
                s.Merge(user);
                study.Acl.Entries.Add(new AccessControlEntry {Permission = readPermisson, Principal = user, AccessControlList = study.Acl});
                s.Save(study);
                s.Flush();
            };

            void AddPersonToStudy(ISession s)
            {
                s.Merge(person);
                person.AddSubjectIdentifier(study, Guid.NewGuid().ToString(), Enumerable.Empty<IIdentity>());
            }

            Db.InTransactionalUnitOfWork(GiveUserAccessToStudy.Then(AddPersonToStudy));

            var people = GetAccessiblePeople();

            Assert.Empty(people);
        }

        private void SavePeople(ISession s)
        {
            s.Save(person);
            s.Save(readPermisson);
            s.Save(user);
        }

        private void AddReadAccessForUser(ISession s)
        {
            person = s.Merge(person);
            person.Acl.Entries.Add(
                new AccessControlEntry
                {
                    Permission = readPermisson,
                    Principal = user,
                    AccessControlList = person.Acl
                });
        }

        private class UserAccessor : IUserAccessor
        {
            private readonly Func<User> getuser;

            public UserAccessor(Func<User> getuser)
            {
                this.getuser = getuser;
            }

            /// <inheritdoc />
            public IUser GetUser() => getuser();
        }
    }
}