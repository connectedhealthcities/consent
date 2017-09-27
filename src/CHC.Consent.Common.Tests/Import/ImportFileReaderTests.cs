using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Datasources;
using CHC.Consent.Identity.Core;
using Xunit;

namespace CHC.Consent.Common.Tests.Import
{
    public static class ImportFileReaderTests
    {
        public class WhenImportingAnEmptySource
        {
            private readonly PersonSpecification[] converted;

            public WhenImportingAnEmptySource()
            {
                converted = new ImportFileReader(new InlineDataSource(new Study()), new InMemoryIdentityKindStore()).ToArray();
            }

            [Fact]
            public void NoRecordsWereConverted()
            {
                Assert.Empty(converted);
            }
        }

        public class WhenImportingASingleRecord
        {
            private readonly PersonSpecification[] converted;

            public WhenImportingASingleRecord()
            {
                converted = new ImportFileReader(new InlineDataSource(new Study(), new XmlImportRecord()), new InMemoryIdentityKindStore()).ToArray();
            }

            [Fact]
            public void OneRecordWasConverted()
            {
                Assert.Equal(1, converted.Length);
            }
        }
    }

    public class InMemoryIdentityKindStore : IIdentityKindStore
    {
        private readonly IDictionary<string, IIdentityKind> identityKinds;
        public IIdentityKind FindIdentityKindByExternalId(string externalId)
        {
            return identityKinds.TryGetValue(externalId, out var found) ? found : null;
        }

        public InMemoryIdentityKindStore(IDictionary<string, IIdentityKind> identityKinds=null)
        {
            this.identityKinds = identityKinds??new Dictionary<string, IIdentityKind>();
        }
    }

    public class InlineDataSource : IStandardDataDatasource
    {
        public InlineDataSource(IStudy study, params IImportRecord[] people)
        {
            People = people;
            Study = study;
        }

        public IEnumerable<IImportRecord> People { get; }
        public IStudy Study { get; }
    }
}