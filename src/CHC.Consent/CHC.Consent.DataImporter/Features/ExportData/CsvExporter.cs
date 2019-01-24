using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CsvHelper;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    public class CsvExporter
    {
        private readonly string[][] fieldNames;

        public CsvExporter(IApi apiClient, string[] fieldNames)
        {
            this.fieldNames = fieldNames.Select(_ => _.Split("::")).ToArray();
            ApiClient = apiClient;
        }

        private IApi ApiClient { get; set; }

        public async Task Export(long studyId, StreamWriter outputWriter)
        {
            var definitions = await ApiClient.GetIdentityStoreMetadataAsync();
            var studySubjects = ApiClient
                .GetConsentedSubjectsForStudy(studyId)
                .Select(
                    studySubject =>
                        new StudySubjectWithIdentifiers
                        {
                            subjectIdentifier = studySubject.SubjectIdentifier,
                            identifiers = ApiClient.GetPerson(studySubject.PersonId.Id.Value)
                        })
                .ToArray();


            Write(definitions, studySubjects, outputWriter);
        }

        public virtual void Write(
            ICollection<IdentifierDefinition> definitions, 
            IEnumerable<StudySubjectWithIdentifiers> studySubjects,
            TextWriter outputWriter)
        {
            var topLevel = new HashSet<string>(fieldNames.Select(_ => _.First()));
            var selectedFields = definitions.Where(_ => topLevel.Contains(_.SystemName)).ToArray();
            var outputFormatter = new ValueOutputFormatter(selectedFields, fieldNames);
            using (var csv = new CsvWriter(outputWriter))
            {
                WriteHeader(csv, selectedFields);

                foreach (var subject in studySubjects)
                {
                    WriteRecord(csv, outputFormatter, subject.subjectIdentifier, subject.identifiers);
                }
            }
        }

        private void WriteRecord(
            IWriter output,
            ValueOutputFormatter outputFormatter,
            string subjectIdentifier,
            IEnumerable<IIdentifierValueDto> values)
        {
            output.WriteField(subjectIdentifier);
            outputFormatter.Write(values, output);
            output.NextRecord();
        }

        private void WriteHeader(IWriter output, IEnumerable<IdentifierDefinition> definitions)
        {
            output.WriteField("id");
            foreach (var name in fieldNames.Select(n => string.Join("::", n)))
            {
                output.WriteField(name);
            }

            output.NextRecord();
        }

        private IEnumerable<string> FieldNames(IEnumerable<IdentifierDefinition> definitions) =>
            new GatherOutputNames(definitions);
    }
}