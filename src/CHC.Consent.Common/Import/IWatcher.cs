using System;
using System.Threading;

namespace CHC.Consent.Common.Import
{
    public interface IWatcher
    {
        
        void Start();
        EventWaitHandle WaitHandle { get; }
        event EventHandler<NewDataEventArgs> NewDatasourceAvailable;
    }
}