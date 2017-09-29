using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Datasources;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.Common.Tests.Import.Utils;
using CHC.Consent.Core;
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
                converted = AReader().GetConvertRecords();
            }

            [Fact]
            public void NoRecordsWereConverted()
            {
                Assert.Empty(converted);
            }
        }

        public class WhenImportingASingleEmptyRecord
        {
            private readonly PersonSpecification[] converted;

            public WhenImportingASingleEmptyRecord()
            {
                converted = AReader()
                    .WithRecords(new XmlImportRecord())
                    .GetConvertRecords();

            }

            [Fact]
            public void OneRecordWasConverted()
            {
                Assert.Equal(1, converted.Length);
            }
        }
        
        public class WhenImportingASingleRecord
        {
            private readonly PersonSpecification[] converted;
            private readonly TestIdentityKind identityKind;

            public WhenImportingASingleRecord()
            {
                identityKind = new TestIdentityKind("external123");

                converted =
                    AReader()
                        .WithIdenityKinds(identityKind)
                        .WithRecords(
                            new XmlImportRecord
                            {
                                Identities =
                                {
                                    new SimpleIdentityRecord(
                                        identityKindExternalId: "external123",
                                        value: "hank")
                                }
                            })
                        .GetConvertRecords();
            }

            [Fact]
            public void OneRecordWasConverted()
            {
                Assert.Equal(1, converted.Length);
            }

            [Fact]
            public void TheConvertedRecordHasOneIdentity()
            {
                Assert.Single(converted[0].Identities);
            }

            [Fact]
            public void TheConvertedIdentityHasCorrectIdentityKindId()
            {
                Assert.Equal(identityKind.Id, FirstIdentity?.IdentityKindId);
            }

            private IIdentity FirstIdentity => converted.FirstOrDefault()?.Identities.FirstOrDefault();

            [Fact]
            public void TheConvertedIdentityHasCorrectValue()
            {
                Assert.Equal("hank", (FirstIdentity as ISimpleIdentity)?.Value);
            }
        }

        public class WhenImportingMatches
        {
            private readonly IIdentityKind identityKind1;
            private readonly IIdentityKind identityKind2;
            private readonly PersonSpecification[] converted;
            private readonly PersonSpecification theRecord;

            public WhenImportingMatches()
            {
                identityKind1 = new TestIdentityKind("external-123");
                identityKind2 = new TestIdentityKind("external-444");

                converted =
                    AReader()
                        .WithIdenityKinds(identityKind1, identityKind2)
                        .WithRecords(
                            new XmlImportRecord
                            {
                                Identities =
                                {
                                    new SimpleIdentityRecord(
                                        identityKindExternalId: identityKind1.ExternalId,
                                        value: "hank"),
                                    new SimpleIdentityRecord(
                                        identityKind2.ExternalId,
                                        value: "roger"
                                    )
                                },
                                MatchIdentity = new[]
                                {
                                    new MatchByIdentityKindIdRecord {IdentityKindExternalId = identityKind1.ExternalId},
                                },
                                MatchStudyIdentity = new[]
                                {
                                    new MatchByIdentityKindIdRecord {IdentityKindExternalId = identityKind2.ExternalId}
                                }
                            }
                        )
                        .GetConvertRecords();
                    
                theRecord = converted[0];
            }

            [Fact]
            public void SpecificationShouldHaveCorrectMatchIdentites()
            {
                Assert.Single(
                    theRecord.MatchIdentity.OfType<IIdentityMatch>(),
                    _ => _.Match.IdentityKindId == identityKind1.Id
                         && ((ISimpleIdentity) _.Match).Value == "hank");
            }

            [Fact]
            public void SpecificationShouldHaveCorrectMatchSubjectIdentities()
            {
                Assert.Single(
                    theRecord.MatchSubjectIdentity.OfType<ISimpleIdentity>(),
                    _ => _.IdentityKindId == identityKind2.Id
                         && _.Value == "roger"
                );
            }
        }

        public class WhenImportingEvidence
        {
            private readonly EvidenceKindStub evidenceKind1;
            private readonly EvidenceKindStub evidenceKind2;
            private readonly ImportFileReader reader;

            /// <inheritdoc />
            public WhenImportingEvidence()
            {
                evidenceKind1 = new EvidenceKindStub("one");
                evidenceKind2 = new EvidenceKindStub("two");

                reader = AReader()
                    .WithEvidenceKinds(evidenceKind1, evidenceKind2)
                    .WithRecords(
                        new XmlImportRecord
                        {
                            Evidence =
                            {
                                new EvidenceRecord
                                {
                                    Evidence = "Some evidence",
                                    EvidenceKindExternalId = evidenceKind1.ExternalId
                                }
                            }
                        }
                    ).Build();
            }

            [Fact]
            public void CanFindEvidenceKinds()
            {
                Assert.Equal(evidenceKind1, reader.FindEvidenceKindByExternalId(evidenceKind1.ExternalId));
            }

            [Fact]
            public void CanConvertEvidence()
            {
                var converted = reader
                    .Convert(
                        new EvidenceRecord
                        {
                            Evidence = "some evidence",
                            EvidenceKindExternalId = evidenceKind1.ExternalId
                        }
                    );
            
                Assert.Equal("some evidence", converted.Evidence);
                Assert.Equal(evidenceKind1.Id, converted.EvidenceKindId);
            }
            
            [Fact]
            public void CanConvertEvidenceEnumerable()
            {
                var converted = reader
                    .Convert(
                        new[]
                        {
                            new EvidenceRecord
                            {
                                Evidence = "some more evidence",
                                EvidenceKindExternalId = evidenceKind1.ExternalId
                            }
                        }
                    );

                Assert.Single(converted);
                Assert.Single(converted, EqualsEvidence("some more evidence", evidenceKind1));
            }
            
            [Fact]
            public void HasOneEvidence()
            {
                var theRecord = reader.SingleOrDefault();
                Assert.Single(theRecord.Evidence);
            }
            
            [Fact]
            public void HasCorrectEvidence()
            {
                var theRecord = reader.SingleOrDefault();
                Assert.Single(
                    theRecord.Evidence,
                    EqualsEvidence("Some evidence", evidenceKind1)
                );
            }

            private static Predicate<IEvidence> EqualsEvidence(string evidence, IEvidenceKind evidenceKind)
            {
                return r =>
                    r.Evidence == evidence
                    && r.EvidenceKindId == evidenceKind.Id;
            }
        }
        
        [Fact]
        public static void InMemoryEvidenceKindStoreReturnsCorrectValues()
        {
            var evidenceKind1 = new EvidenceKindStub();
            var evidenceKind2 = new EvidenceKindStub();

            var store = new InMemoryEvidenceKindStore(evidenceKind1,evidenceKind2);
                
            Assert.Equal(evidenceKind1, store.FindEvidenceKindByExternalId(evidenceKind1.ExternalId));
            Assert.Equal(evidenceKind2, store.FindEvidenceKindByExternalId(evidenceKind2.ExternalId));
            Assert.Null(store.FindEvidenceKindByExternalId("DOES NOT EXIST"));
        }

        [Fact]
        public static void ReaderCanConvertSingleEvidence()
        {
            
        }
        
        private class ImportFileReaderBuilder
        {
            private IImportRecord[] records = new IImportRecord[0];
            private IIdentityKind[] identityKinds = new IIdentityKind[0];
            private IStudy study = new StudyStub();
            private IEvidenceKind[] evidenceKinds = new IEvidenceKind[0];

            public ImportFileReaderBuilder WithRecords(params IImportRecord[] newRecords)
            {
                return CopyAndChange(_ => _.records = newRecords);
            }

            public ImportFileReaderBuilder WithIdenityKinds(params IIdentityKind[] newIdentityKinds)
            {
                return CopyAndChange(_ => _.identityKinds = newIdentityKinds);
            }

            private ImportFileReaderBuilder CopyAndChange(Action<ImportFileReaderBuilder> update)
            {
                var copy = Copy();
                update(copy);
                return copy;
            }
            
            private ImportFileReaderBuilder Copy()
            {
                return new ImportFileReaderBuilder
                {
                    records = records,
                    identityKinds = identityKinds,
                    study = study,
                    evidenceKinds = evidenceKinds
                };
            }

            public ImportFileReader Build()
            {
                return new ImportFileReader(
                    new InlineDataSource(new StudyStub(), records),
                    new InMemoryIdentityKindStore(identityKinds),
                    new InMemoryEvidenceKindStore(evidenceKinds));
            }

            public PersonSpecification[] GetConvertRecords()
            {
                return Build().ToArray();
            }

            public ImportFileReaderBuilder WithEvidenceKinds(params IEvidenceKind[] newEvidenceKinds)
            {
                return CopyAndChange(_ => _.evidenceKinds = newEvidenceKinds);
            }
        }

        private static ImportFileReaderBuilder AReader() => new ImportFileReaderBuilder();
    }

    internal class InMemoryEvidenceKindStore : IEvidenceKindStore
    {
        private readonly IDictionary<string, IEvidenceKind> evidenceKinds;
        public IEvidenceKind FindEvidenceKindByExternalId(string externalId)
        {
            return evidenceKinds.TryGetValue(externalId, out var found) ? found : null;
        }

        public InMemoryEvidenceKindStore(params IEvidenceKind[] identityKinds)
        {
            this.evidenceKinds = identityKinds.ToDictionary(_ => _.ExternalId);
        }
    }

    public class EvidenceKindStub : IEvidenceKind
    {
        /// <inheritdoc />
        public EvidenceKindStub(string externalIdExtension=null)
        {
            ExternalId = CreateExternalId(externalIdExtension ?? Guid.NewGuid().ToString());
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string ExternalId { get; } 

        private static string CreateExternalId(string extension=null)
        {
            return $"urn:chc:consent:evidence-kind:test:{extension}";
        }
    }


    public class TestIdentityKind : IIdentityKind
    {
        public Guid Id { get; }
        public string ExternalId { get; }

        public TestIdentityKind(string externalId)
        {
            ExternalId = externalId;
            Id = Guid.NewGuid();
        }
    }

    public class InMemoryIdentityKindStore : IIdentityKindStore
    {
        private readonly IDictionary<string, IIdentityKind> identityKinds;
        public IIdentityKind FindIdentityKindByExternalId(string externalId)
        {
            return identityKinds.TryGetValue(externalId, out var found) ? found : null;
        }

        public InMemoryIdentityKindStore(params IIdentityKind[] identityKinds)
        {
            this.identityKinds = identityKinds.ToDictionary(_ => _.ExternalId);
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