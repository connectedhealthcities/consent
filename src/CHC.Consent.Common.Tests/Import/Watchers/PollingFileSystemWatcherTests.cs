using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Datasources;
using CHC.Consent.Common.Import.Watchers;
using CHC.Consent.Common.Tests.Import.Utils;
using Xunit;

namespace CHC.Consent.Common.Tests.Import.Watchers
{
    public class PollingFileSystemWatcherTests
    {
        class PollingFileSystemWatcherStub : PollingFileSystemWatcher
        {
            private readonly CancellationTokenSource cancellationTokenSource;
            public Queue<string[]> Files { get; } = new Queue<string[]>();
            
            public PollingFileSystemWatcherStub(CancellationTokenSource cancellationTokenSource) 
                : base("", new StudyStub(), TimeSpan.MaxValue, cancellationTokenSource.Token)
            {
                this.cancellationTokenSource = cancellationTokenSource;
            }

            protected override void Wait()
            {
                
            }

            protected override string[] GetFiles()
            {
                var nextFiles = Files.Dequeue();
                if(Files.Count == 0) cancellationTokenSource.Cancel();
                return nextFiles;
            }
        }

        private static NewDataEventArgs[] RunWatcher(params string[][] fileLists)
        {
            var newData = new BlockingCollection<NewDataEventArgs>();
            var watcher = new PollingFileSystemWatcherStub(new CancellationTokenSource());

            watcher.NewDatasourceAvailable += (_, e) => newData.Add(e);
            
            foreach (var fileList in fileLists)
            {
                watcher.Files.Enqueue(fileList);
            }
            
            
            watcher.Start();

            Assert.True(watcher.WaitHandle.WaitOne(TimeSpan.FromSeconds(1)), "Watcher took more than a second");

            return newData.ToArray();
        }

        private static string[] NoFiles { get; } = new string[0];
        
        private static string[] FileList(params string[] files)
        {
            return files;
        }

        [Fact]
        public void DoesNotNotifyWhenThereAreNoFiles()
        {
            var raised = RunWatcher(NoFiles,NoFiles);
            
            Assert.Empty(raised);
        }

        [Fact]
        public void DoesNotNotifyWhenFilesDontChange()
        {
            var raised = RunWatcher(FileList("a"),FileList("a"));
            
            Assert.Empty(raised);
        }
        
        
        [Fact]
        public void DoesNotNotifyWhenFilesAreRemoved()
        {
            var raised = RunWatcher(FileList("a"),NoFiles);
            
            Assert.Empty(raised);
        }
        
        [Fact]
        public void NotifiesWhenFileAdded()
        {
            var raised = RunWatcher(NoFiles, FileList("a"));
            
            Assert.Single(raised);
        }
        
        [Fact]
        public void RepeatedlyNotifiesWhenNewFileAdded()
        {
            var raised = RunWatcher(NoFiles, FileList("a"), FileList("b"));
            
            Assert.Equal(2, raised.Length);
        }

        [Fact]
        public void RaisedNotificationHasCorrectFileName()
        {
            const string expectedFileName = "a";
            var raised = RunWatcher(NoFiles, FileList(expectedFileName));

            var fileImporter = (FileDatasource) raised[0].Datasource;
            
            Assert.Equal(expectedFileName, fileImporter.FileLocation);
        }
    }
}