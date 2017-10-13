using System;
using System.Linq;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Consent;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.NHibernate.WebApi;
using CHC.Consent.Testing.NHibernate;
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
        public DatabaseFixture Db { get; }

        /// <inheritdoc />
        public SecurePersonRepositoryTests(DatabaseFixture db, ITestOutputHelper output)
        {
            Db = db;
            Db.StartSession(output.WriteLine).Dispose();
            
            LoggerProvider.SetLoggersFactory(new OutputLoggerFactory(output));
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
            var user = new User();
            var person = new PersistedPerson(Enumerable.Empty<PersistedIdentity>());

            var read = new Permisson {Name = "read"};

            void SavePeople(ISession s)
            {
                s.Save(person);
                s.Save(read);

                var securablePerson = new SecurablePerson {Person = person};
                s.Save(securablePerson);

                user.PermissionEntries.Add(
                    new PermissionEntry
                    {
                        Permisson = read,
                        Principal = user,
                        Securable = securablePerson
                    });

                s.Save(user);
            }
            
            Db.InTransactionalUnitOfWork(SavePeople);


            var securePersonRepository = new SecurePersonRepository(UserAccessor(user), Db.SessionAccessor);


            var people = Db.InTransactionalUnitOfWork(
                () => securePersonRepository
                    .GetPeople()

                    .ToArray());
            
            Assert.NotEmpty(people);
        }

        private static IUserAccessor UserAccessor(User user)
        {
            var getUser = new Mock<IUserAccessor>();
            getUser.Setup(_ => _.GetUser()).Returns(user);
            return getUser.Object;
        }

        [Fact]
        public void CanProjectPeople()
        {
            var user = new User();
            var person = new PersistedPerson(Enumerable.Empty<PersistedIdentity>());

            var read = new Permisson {Name = "read"};

            void SavePeople(ISession s)
            {
                s.Save(person);
                s.Save(read);

                var securablePerson = new SecurablePerson {Person = person};
                s.Save(securablePerson);

                user.PermissionEntries.Add(
                    new PermissionEntry
                    {
                        Permisson = read,
                        Principal = user,
                        Securable = securablePerson
                    });

                s.Save(user);
            }
            
            Db.InTransactionalUnitOfWork(SavePeople);

            var getUser = new Mock<IUserAccessor>();
            getUser.Setup(_ => _.GetUser()).Returns(user);


            var securePersonRepository = new SecurePersonRepository(getUser.Object, Db.SessionAccessor);


            var people = Db.InTransactionalUnitOfWork(
                () => securePersonRepository
                    .GetPeople()
                    .Skip(0).Take(10)
                    .Select(p => new PersonProjection{ Id = p.Id})
                    .ToArray());
            
            Assert.NotEmpty(people);
        }


        [Fact]
        public void HasAccessToPeopleViaStudy()
        {
            var user = new User();
            
            var study = new Study();
            var person = new PersistedPerson(Enumerable.Empty<PersistedIdentity>());
            var read = new Permisson {Name = "read"};
            
            Db.InTransactionalUnitOfWork(
                s =>
                {
                    s.Save(study);
                    person.AddSubjectIdentifier(study, Guid.NewGuid().ToString(), Enumerable.Empty<IIdentity>());
                    s.Save(person);
                    s.Save(read);
                    
                    var securable = new SecurableStudy {Study = study};
                    s.Save(securable);

                    user.PermissionEntries.Add(
                        new PermissionEntry
                        {
                            Permisson = read,
                            Principal = user,
                            Securable = securable
                        });

                    s.Save(user);

                }
            );

            

            var securePersonRepository = new SecurePersonRepository(UserAccessor(user), Db.SessionAccessor);

            var people = Db.InTransactionalUnitOfWork(
                () => securePersonRepository
                    .GetPeople()
                    .ToArray()
            );

            Assert.NotEmpty(people);
        }
    }
}