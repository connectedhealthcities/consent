using System;
using System.Collections.Generic;
using System.IO;
using CHC.Consent.Api.Client.Models;
using CsvHelper;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    public class StudySubjectCsvWriter
    {
        private class Writer
        {
            public Writer(IWriter csv, ValueOutputFormatter outputFormatter)
            {
                Csv = csv;
                OutputFormatter = outputFormatter;
            }

            private IWriter Csv { get; set; }
            private ValueOutputFormatter OutputFormatter { get; set; }

            public void Write(StudySubjectWithIdentifiers subject)
            {
                Csv.WriteField(subject.subjectIdentifier);
                OutputFormatter.Write(subject.identifiers, Csv);
                Csv.NextRecord();
            }
        }

        public StudySubjectCsvWriter(Func<TextWriter> createOutputWriter)
        {
            CreateOutputWriter = createOutputWriter;
        }

        private Func<TextWriter> CreateOutputWriter { get; }

        private static void WriteHeader(IWriter output, string[][] strings)
        {
            output.WriteField("id");
            foreach (var name in FieldNameList.FullFieldNames(strings))
            {
                output.WriteField(name);
            }

            output.NextRecord();
        }

        public void Write(
            IList<IdentifierDefinition> definitions,
            string[][] fieldNames,
            IEnumerable<StudySubjectWithIdentifiers> studySubjects)
        {
            var outputFormatter = new ValueOutputFormatter(definitions, fieldNames);
            using (var csv = new CsvWriter(CreateOutputWriter()))
            {
                var writer = new Writer(csv, outputFormatter);
                WriteHeader(csv, fieldNames);

                foreach (var subject in studySubjects)
                {
                    writer.Write(subject);
                }
            }
        }
    }
}