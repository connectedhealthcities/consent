using System.Collections.Generic;
using CHC.Consent.Common.Core;

namespace CHC.Consent.Common.Import
{
    public interface IStandardDataDatasource
    {
        IEnumerable<IImportRecord> People { get; }
        IStudy Study { get; }
    }
}