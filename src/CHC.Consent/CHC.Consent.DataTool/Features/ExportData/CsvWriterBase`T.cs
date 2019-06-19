using System;
using System.Collections.Generic;
using System.IO;
using CHC.Consent.Api.Client.Models;
using CsvHelper;

namespace CHC.Consent.DataTool.Features.ExportData
{
    public abstract class CsvWriterBase<T> where T : IHaveIdentifiers
    {
        protected CsvWriterBase(Func<TextWriter> createOutputWriter)
        {
            CreateOutputWriter = createOutputWriter;
        }

        private Func<TextWriter> CreateOutputWriter { get; }

        protected abstract string GetId(T haveIdentifiers);

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
            IEnumerable<IdentifierDefinition> definitions,
            string[][] fieldNames,
            IEnumerable<T> studySubjects)
        {
            var outputFormatter = new IdentifierValueOutputFormatter(definitions, fieldNames);
            using (var csv = new CsvWriter(CreateOutputWriter()))
            {
                WriteHeader(csv, fieldNames);
                var writer = new Writer(csv, GetId, outputFormatter);

                foreach (var subject in studySubjects)
                {
                    writer.Write(subject);
                }
            }
        }

        private class Writer
        {
            public Writer(IWriter csv, Func<T, string> getId, IdentifierValueOutputFormatter outputFormatter)
            {
                Csv = csv;
                GetId = getId;
                OutputFormatter = outputFormatter;
            }

            private IWriter Csv { get; set; }
            private Func<T, string> GetId { get; }
            private IdentifierValueOutputFormatter OutputFormatter { get; set; }

            public void Write(T subject)
            {
                Csv.WriteField(GetId(subject));
                OutputFormatter.Write(subject.Identifiers, Csv);
                Csv.NextRecord();
            }
        }
    }
}