using System;
using System.IO;

namespace CHC.Consent.DataTool.Features.ExportData
{
    public class StudySubjectCsvWriter : CsvWriterBase<StudySubjectWithIdentifiers>
    {
        public StudySubjectCsvWriter(Func<TextWriter> createOutputWriter) : base(createOutputWriter)
        {
        }

        protected override string GetId(StudySubjectWithIdentifiers subject)
        {
            return subject.subjectIdentifier;
        }
    }
}