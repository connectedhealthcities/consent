using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Watchers;
using Xunit;

namespace CHC.Consent.IntegrationTests
{
    public class TestImportingFiles
    {
        [Fact]
        public void TestImportingXmlFiles()
        {
            var mailDrop = Path.Combine(Environment.CurrentDirectory, "Import");
            
            using (var system = new ConsentSystem())
            {
                system.WatchFileSystem(mailDrop);

                File.Copy(
                    Path.Combine(Environment.CurrentDirectory, @"data\import.xml"),
                    Path.Combine(mailDrop, $"import-{DateTime.UtcNow:yyyyMMddThhmmss}.xml"));
                
                
            }

        }
    }

    public class ConsentSystem : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        private BlockingCollection<IWatcher> Watchers { get; } = new BlockingCollection<IWatcher>();

        public ConsentSystem()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
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
            
            this.Watchers.Add(watcher, CancellationToken);
            watcher.NewDatasourceAvailable += HandleNewData;
            watcher.Start();
        }

        private void HandleNewData(object sender, NewDataEventArgs e)
        {
            
        }
    }

    public static class ConsentSystemExtensions
    {
        public static void WatchFileSystem(this ConsentSystem @this, string location)
        {
            @this.AddWatcher(new PollingFileSystemWatcher(location, TimeSpan.FromSeconds(10), @this.CancellationToken));
        }
    }
}