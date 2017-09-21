using System.Collections.Generic;
using CHC.Consent.Common.Core;

namespace CHC.Consent.Common.Import
{
    public interface IStandardDataDatasource
    {
        IEnumerable<IPerson> People { get; }
        IStudy Study { get; }
    }
}