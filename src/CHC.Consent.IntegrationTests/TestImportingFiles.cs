using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Watchers;
using CHC.Consent.Common.SubjectIdentifierCreation;
using CHC.Consent.Identity.Core;
using CHC.Consent.Identity.SimpleIdentity;
using CHC.Consent.NHibernate;
using CHC.Consent.NHibernate.Consent;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.Testing.NHibernate;
using NHibernate.Linq;
using Xunit;
using ISessionFactory = CHC.Consent.NHibernate.ISessionFactory;
using Study = CHC.Consent.NHibernate.Consent.Study;

namespace CHC.Consent.IntegrationTests
{
    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
    [Collection("Database")]
    public class TestImportingFiles
    {
        private readonly DatabaseFixture db;

        /// <inheritdoc />
        public TestImportingFiles(DatabaseFixture db)
        {
            this.db = db;
        }

        [Fact]
        public void TestImportingXmlFiles()
        {
            var mailDropLocation = Path.Combine(Environment.CurrentDirectory, "Import");
            Directory.CreateDirectory(mailDropLocation);
            var subjectIdentifiers = new SimpleSubjectIdentifierAllocator();
            IStudy study;
            IEvidenceKind evidenceKind;

            IIdentityKind identityKind1;
            IIdentityKind identityKind2;
            
            using (var system = new ConsentSystem(db))
            {
                study = system.CreateStudy();

                identityKind1 = system.AddIdentityKind(externalId:"SimpleKind");
                identityKind2 = system.AddIdentityKind(externalId: "SimpleKind2");

                evidenceKind = system.AddEvidenceKind(externalId: "EvidenceKind1");

                
                system.UseSubjectIdentifiersFrom(subjectIdentifiers);
                
                
                system.WatchFileSystem(mailDropLocation, study);

                File.Copy(
                    Path.Combine(Environment.CurrentDirectory, @"data\import.xml"),
                    Path.Combine(mailDropLocation, $"import-{DateTime.UtcNow:yyyyMMddThhmmss}.xml"));

                Assert.True(system.WaitForAnImportToComplete(TimeSpan.FromSeconds(30)));
            }

            using (var session = db.StartSession())
            {
                var imported = session.Query<NHibernate.Consent.Consent>().FirstOrDefault(_ => _.SubjectIdentifier == subjectIdentifiers.LastId);

                Assert.Equal(study.Id, imported.StudyId);
                Assert.Single(imported.ProvidedEvidence);
                Assert.Equal("The evidence that was sent", imported.ProvidedEvidence.First().TheEvidence);
                Assert.Equal(evidenceKind.Id, imported.ProvidedEvidence.First().EvidenceKindId);
            }

            using (var session = db.StartSession())
            {
                var identity = session.Query<PersistedPerson>().FirstOrDefault();
                
                Assert.NotEmpty(identity.Identities);

                Assert.Single(
                    identity.Identities,
                    _ => _.IdentityKindId == identityKind1.Id && ((ISimpleIdentity) _).Value == "111-111-111");

                Predicate<PersistedIdentity> isIdentity2 = _ => _.IdentityKindId == identityKind2.Id && ((ISimpleIdentity) _).Value == "222";
                Assert.Single(identity.Identities,isIdentity2);

                Assert.Single(identity.SubjectIdentifiers);

                var subjectIdentifier = identity.SubjectIdentifiers.Single();
                Assert.Equal(subjectIdentifiers.LastId, subjectIdentifier.SubjectIdentifier);
                Assert.Equal(study.Id, subjectIdentifier.StudyId);
                Assert.Single(subjectIdentifier.Identities);
                Assert.Contains(subjectIdentifier.Identities, isIdentity2);
            }
        }
    }

    public class SimpleSubjectIdentifierAllocator : ISubjectIdentfierAllocator
    {
        private int nextId = 0;
        private string lastId;

        public string AllocateNewIdentifier(IStudy study)
        {
            lastId = $"{Interlocked.Increment(ref nextId):X8}";
            return lastId;
        }

        /// <inheritdoc />
        public SimpleSubjectIdentifierAllocator(int seed=0)
        {
            nextId = seed;
        }

        public string LastId => lastId;
    }

    public class ConsentSystem : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private ISubjectIdentfierAllocator subjectIdentfierAllocator;
        private IIdentityStore identityStore;
        private readonly NHibernateConsentStore consentStore;
        
        private readonly IdentityKindStore identityKindStore;
        private ISessionFactory dbSessionFactory;
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        private BlockingCollection<IWatcher> Watchers { get; } = new BlockingCollection<IWatcher>();
        public event EventHandler DatasourceImported;

        public ConsentSystem(ISessionFactory fixture)
        {
            cancellationTokenSource = new CancellationTokenSource();

            dbSessionFactory = fixture;
            
            identityStore = new NHibernateIdentityStore(dbSessionFactory, new NaiveIdentityKindProviderHelper());
            consentStore = new NHibernateConsentStore(dbSessionFactory);
            identityKindStore = new IdentityKindStore(dbSessionFactory);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            cancellationTokenSource.Cancel();
            if (Watchers.Any())
            {
                WaitHandle.WaitAll(
                    Watchers.Select(_ => _.WaitHandle).Cast<WaitHandle>().ToArray(),
                    TimeSpan.FromSeconds(5));
            }
            cancellationTokenSource.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void AddWatcher(IWatcher watcher)
        {
            
            Watchers.Add(watcher, CancellationToken);
            watcher.NewDatasourceAvailable += HandleNewData;
            watcher.Start();
        }

        private void HandleNewData(object sender, NewDataEventArgs e)
        {
            Import(e.Datasource);
        }

        private void Import(IStandardDataDatasource datasource)
        {
            foreach (var specification in Transform(datasource))
            {
                if (IsNewPerson(specification))
                {
                    var person = CreateNewPerson(specification);
                    
                    var subjectIdentifer = GetNewSubjectIdentifer(datasource.Study);

                    var personIdentifier = StoreSubjectIdentifier(datasource.Study, person, subjectIdentifer, specification.MatchSubjectIdentity);

                    RecordConsentFor(personIdentifier, specification.Evidence);
                }
                else
                {
                    //TODO: handle person updates?
                }
            }

            DatasourceImported?.Invoke(this, EventArgs.Empty);
        }

        private ISubjectIdentifier StoreSubjectIdentifier(
            IStudy study,
            IPerson person,
            string subjectIdentifer,
            List<IIdentity> identities) =>
            dbSessionFactory.AsTransaction(
                _ =>
                {
                    person = _.Merge(person);
                    return person.AddSubjectIdentifier(study, subjectIdentifer, identities);
                });
        

        private ImportFileReader Transform(IStandardDataDatasource datasource)
        {
            return new ImportFileReader(datasource, identityKindStore, consentStore);
        }

        private IPerson CreateNewPerson(PersonSpecification specification)
        {
            return identityStore.CreatePerson(specification.Identities);
        }

        private void RecordConsentFor(ISubjectIdentifier subjectIdentifer, IReadOnlyList<IEvidence> evidence)
        {
            //TODO: record consent for identifier
            consentStore.RecordConsent(subjectIdentifer.StudyId, subjectIdentifer.SubjectIdentifier, evidence.AsEnumerable());
        }

        private string GetNewSubjectIdentifer(IStudy study)
        {
            return subjectIdentfierAllocator.AllocateNewIdentifier(study);
        }

        private bool IsNewPerson(PersonSpecification specification)
        {
            return identityStore.FindPerson(specification.MatchIdentity) == null;
        }

        public IStudy CreateStudy()
        {
            return dbSessionFactory.AsTransaction(
                session =>
                {
                    var study = new Study();
                    session.Save(study);
                    return study;
                }
            );
        }

        public void UseSubjectIdentifiersFrom(ISubjectIdentfierAllocator allocator) => subjectIdentfierAllocator = allocator;

        public IIdentityKind AddIdentityKind(string externalId) => identityKindStore.AddIdentity(externalId);
        

        public IEvidenceKind AddEvidenceKind(string externalId) => consentStore.AddEvidenceKind(externalId);

        public bool WaitForAnImportToComplete(TimeSpan fromSeconds)
        {
            var imported = new ManualResetEventSlim(false);
            DatasourceImported += (sender, args) => imported.Set();

            return imported.Wait(fromSeconds);
        }
        
    }

    public static class ConsentSystemExtensions
    {
        public static void WatchFileSystem(this ConsentSystem @this, string location, IStudy study)
        {
            @this.AddWatcher(new PollingFileSystemWatcher(location, study, TimeSpan.FromSeconds(10), @this.CancellationToken));
        }
    }
}