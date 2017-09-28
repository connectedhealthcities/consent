using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Threading;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Evidence;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Watchers;
using CHC.Consent.Common.SubjectIdentifierCreation;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate;
using CHC.Consent.NHibernate.Identity;
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
                
                
                system.WatchFileSystem(mailDropLocation, study);

                File.Copy(
                    Path.Combine(Environment.CurrentDirectory, @"data\import.xml"),
                    Path.Combine(mailDropLocation, $"import-{DateTime.UtcNow:yyyyMMddThhmmss}.xml"));
            }

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
        private IIdentityStore identityStore;
        private readonly Configuration dbSessionFactory;
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        private BlockingCollection<IWatcher> Watchers { get; } = new BlockingCollection<IWatcher>();

        public ConsentSystem()
        {
            cancellationTokenSource = new CancellationTokenSource();
            dbSessionFactory = new Configuration(
                Configuration.SqlServer(@"Data Source=(localdb)\.;Integrated Security=true"));
            
            dbSessionFactory.Create();
            
            identityStore = new NHibernateIdentityStore(dbSessionFactory);
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
            Import(e.Datasource);
        }

        private void Import(IStandardDataDatasource datasource)
        {
            foreach (var specification in new ImportFileReader(datasource, new IdentityKindStore(dbSessionFactory)))
            {
                if (IsNewPerson(specification))
                {
                    var person = CreateNewPerson(specification);
                    
                    var subjectIdentifer = GetNewSubjectIdentifer(datasource.Study);

                    StoreSubjectIdentifier(datasource.Study, person, subjectIdentifer);

                    RecordConsentFor(subjectIdentifer, specification.Evidence);
                }
                else
                {
                    //TODO: handle person updates?
                }
            }
        }

        private IPerson CreateNewPerson(PersonSpecification specification)
        {
            return identityStore.CreatePerson(specification.Identities);
        }

        private static void RecordConsentFor(ISubjectIdentifier subjectIdentifer, IReadOnlyList<Evidence> importRecord)
        {
            //TODO: record consent for identifier
            throw new NotImplementedException();
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

        public void UseSubjectIdentifiersFrom(ISubjectIdentfierAllocator allocator)
        {
            subjectIdentfierAllocator = allocator; 
        }

        public void UseIdentityStore(IIdentityStore store)
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