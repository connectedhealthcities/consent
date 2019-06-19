using System;
using System.IO;
using System.Threading.Tasks;

namespace CHC.Consent.DataTool.Features.ExportData
{
    public abstract class CsvExporter
    {
        public abstract Task Export(long studyId, Func<TextWriter> createOutput);
    }
}