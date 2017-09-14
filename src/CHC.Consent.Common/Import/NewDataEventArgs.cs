using System;

namespace CHC.Consent.Common.Import
{
    public class NewDataEventArgs : EventArgs
    {
        public IStandardDataDatasource Datasource { get; }

        public NewDataEventArgs(IStandardDataDatasource datasource)
        {
            Datasource = datasource;
        }
    }
}