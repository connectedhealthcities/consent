using System;
using System.Linq;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Consent;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.NHibernate.WebApi;
using CHC.Consent.Testing.NHibernate;
using CHC.Consent.Utils;
using CHC.Consent.WebApi.Abstractions;
using Moq;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.NHibernate.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class SecurePersonRepositoryTests
    {
        private readonly User user;
        private readonly Person person;
        private readonly Permisson readPermisson = new Permisson {Name = "read"};
        private readonly SecurePersonRepository securePersonRepository;
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
            securePersonRepository = new SecurePersonRepository(UserAccessor(user), Db.SessionAccessor);
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
        public void HasAccessToPeopleViaExplicitAccess()
        {
            var people = Db.InTransactionalUnitOfWork(
                () => securePersonRepository
                    .GetPeople()
                    .ToArray());
            
            Assert.NotEmpty(people);
        }

        [Fact]
        public void CanProjectPeople()
        {
            var people = Db.InTransactionalUnitOfWork(
                () => securePersonRepository
                    .GetPeople()
                    .Skip(0).Take(10)
                    .Select(p => new PersonProjection{ Id = p.Id})
                    .ToArray());
            
            Assert.NotEmpty(people);
        }

        [Fact]
        public void AccessToStudyDoesNotImplyAccessToPerson()
        {
            var study = new Study();

            void GiveUserAccessToStudy(ISession s)
            {
                s.Merge(readPermisson);
                s.Merge(user);
                study.Acl.Permissions.Add(new AccessControlEntry{Permisson = readPermisson, Principal = user});
                s.Save(study);
                s.Flush();
                
            }

            void AddPersonToStudy(ISession s)
            {
                s.Merge(person);
                person.AddSubjectIdentifier(study, Guid.NewGuid().ToString(), Enumerable.Empty<IIdentity>());
            }

            Action<ISession> removeAccessToPerson = s =>
            {
                s.Merge(person);
                s.Merge(person.Acl);
                person.Acl.Permissions.Clear();
            };

            Db.InTransactionalUnitOfWork(removeAccessToPerson.Then(GiveUserAccessToStudy).Then(AddPersonToStudy));
                
            var people = Db.InTransactionalUnitOfWork(
                () => securePersonRepository
                    .GetPeople()
                    .ToArray());

            Assert.Empty(people);
        }

        private void SavePeople(ISession s)
        {
            s.Save(person);
            s.Save(readPermisson);
            s.Save(user);
            
            person.Acl.Permissions.Add(
                new AccessControlEntry
                {
                    Permisson = readPermisson,
                    Principal = user,
                    AccessControlList = person.Acl
                });

            s.Flush();
        }

        private static IUserAccessor UserAccessor(User user)
        {
            var getUser = new Mock<IUserAccessor>();
            getUser.Setup(_ => _.GetUser()).Returns(user);
            return getUser.Object;
        }
    }
}