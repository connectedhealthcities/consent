using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Threading;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Watchers;
using CHC.Consent.Common.SubjectIdentifierCreation;
using CHC.Consent.NHibernate;
using Xunit;

namespace CHC.Consent.IntegrationTests
{
    public class TestImportingFiles
    {
        [Fact]
        public void TestImportingXmlFiles()
        {
            var mailDropLocation = Path.Combine(Environment.CurrentDirectory, "Import");
            Directory.CreateDirectory(mailDropLocation);
            
            using (var system = new ConsentSystem())
            {
                var study = system.CreateStudy();

                system.UseSubjectIdentifiersFrom(new SimpleSubjectIdentifierAllocator());
                system.UseIdentityStore(new SimpleIdentityStore());
                
                system.WatchFileSystem(mailDropLocation, study);

                File.Copy(
                    Path.Combine(Environment.CurrentDirectory, @"data\import.xml"),
                    Path.Combine(mailDropLocation, $"import-{DateTime.UtcNow:yyyyMMddThhmmss}.xml"));
            }

        }
    }

    public class SimpleIdentityStore : IIdentityStore
    {
        private readonly Dictionary<string, Identity[]> store = new Dictionary<string, Identity[]>(); 
        
        public IEnumerable<Identity> FindExisitingIdentiesFor(IEnumerable<Identity> identies)
        {
            return store.TryGetValue(Key(identies), out var found) ? found : null;
        }

        private static string Key(IEnumerable<Identity> keys)
        {
            return string.Join("\x241f", keys.Cast<SimpleIdentity>().Select(_ => _.Value));
        }

        public void UpsertIdentity(IEnumerable<Identity> keys, IEnumerable<Identity> identities)
        {
            var key = Key(keys);
            store[key] = identities.ToArray();
        }
    }

    public class SimpleSubjectIdentifierAllocator : ISubjectIdentfierAllocator
    {
        private int nextId = 0;
        public string AllocateNewIdentifier(IStudy study)
        {    
            return $"{Interlocked.Increment(ref nextId):X8}";
        }
    }

    public class ConsentSystem : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private ISubjectIdentfierAllocator subjectIdentfierAllocator;
        private SimpleIdentityStore identityStore;
        private readonly Configuration dbSessionFactory;
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        private BlockingCollection<IWatcher> Watchers { get; } = new BlockingCollection<IWatcher>();

        public ConsentSystem()
        {
            cancellationTokenSource = new CancellationTokenSource();
            dbSessionFactory = new Configuration(
                Configuration.SqlServer(@"Data Source=(localdb)\.;Integrated Security=true"));
            dbSessionFactory.Create();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            dbSessionFactory.DropSchema();
            cancellationTokenSource.Cancel();
            WaitHandle.WaitAll(
                Watchers.Select(_ => _.WaitHandle).Cast<WaitHandle>().ToArray(),
                TimeSpan.FromSeconds(5));
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
            var study = e.Datasource.Study;
            var people = e.Datasource.People;
            Import(people, study);
        }

        private void Import(IEnumerable<IPerson> people, IStudy study)
        {
            foreach (var person in people)
            {
                if (IsNewPerson(person.Identities))
                {
                    var subjectIdentifer = GetNewSubjectIdentifer(study);

                    RecordConsentFor(subjectIdentifer, person);
                }
                else
                {
                    //TODO: handle updates?
                }
            }
        }

        private static void RecordConsentFor(ISubjectIdentifier subjectIdentifer, IPerson person)
        {
            //TODO: record consent for identifier
        }

        private ISubjectIdentifier GetNewSubjectIdentifer(IStudy study)
        {
            return new SubjectIdentifier(study, subjectIdentfierAllocator.AllocateNewIdentifier(study));
        }

        private static bool IsNewPerson(IDictionary<IdentityKind, Identity> personIdentities)
        {
            //TODO: check for subjects by identity
            throw new NotImplementedException();
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

        public void UseSubjectIdentifiersFrom(ISubjectIdentfierAllocator allocator)
        {
            subjectIdentfierAllocator = allocator; 
        }

        public void UseIdentityStore(SimpleIdentityStore store)
        {
            identityStore = store;
            
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