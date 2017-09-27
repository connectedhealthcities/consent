using System.Collections.Generic;

namespace CHC.Consent.Common.Import
{
    public abstract class StandardDataReader
    {
        public abstract IEnumerable<IImportRecord> People();
    }
}