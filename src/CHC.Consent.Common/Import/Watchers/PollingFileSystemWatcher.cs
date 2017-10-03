using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Import.Datasources;
using CHC.Consent.Utils;

namespace CHC.Consent.Common.Import.Watchers
{
    public class PollingFileSystemWatcher : IWatcher
    {
        private readonly string location;
        private readonly IStudy study;
        private readonly TimeSpan pollingInterval;
        private readonly CancellationToken cancellationToken;

        public PollingFileSystemWatcher(
            string location, 
            IStudy study,
            TimeSpan pollingInterval, 
            CancellationToken cancellationToken)
        {
            this.location = location;
            this.study = study;
            this.pollingInterval = pollingInterval;
            this.cancellationToken = cancellationToken;
        }

        public void Start()
        {
            WaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            Task.Run(
                () =>
                {
                    var files = GetFiles();
                    while (!ShouldStopProcessing())
                    {
                        Wait();
                        if (ShouldStopProcessing()) return;

                        var currentFiles = GetFiles();

                        var newFiles = currentFiles.Except(files).ToArray();

                        files = currentFiles;
                        
                        foreach (var newFile in newFiles)
                        {
                            NewDatasourceAvailable.Trigger(this, new NewDataEventArgs(new FileDatasource(newFile, study)));
                        }
                    }
                },
                cancellationToken)
                .ContinueWith(t => WaitHandle.Set(), TaskContinuationOptions.LazyCancellation);
        }

        private bool ShouldStopProcessing()
        {
            return cancellationToken.IsCancellationRequested;
        }

        protected virtual void Wait()
        {
            cancellationToken.WaitHandle.WaitOne(pollingInterval);
        }

        protected virtual string[] GetFiles()
        {
            return Directory.GetFiles(location);
        }

        public EventWaitHandle WaitHandle { get; private set; }
        public event EventHandler<NewDataEventArgs> NewDatasourceAvailable;
    }
}