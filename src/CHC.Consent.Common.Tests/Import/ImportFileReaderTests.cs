using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Datasources;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.Common.Tests.Import.Utils;
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
                converted = new ImportFileReader(new InlineDataSource(new StudyStub()), new InMemoryIdentityKindStore()).ToArray();
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
                converted = new ImportFileReader(new InlineDataSource(
                    new StudyStub(), 
                    new XmlImportRecord()), new InMemoryIdentityKindStore()).ToArray();
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
                
                converted = new ImportFileReader(
                        new InlineDataSource(
                            new StudyStub(),
                            new XmlImportRecord
                            {
                                Identities =
                                {
                                    new SimpleIdentityRecord(
                                        identityKindExternalId: "external123",
                                        value: "hank")
                                }
                            }),
                        new InMemoryIdentityKindStore(identityKind))
                    .ToArray();
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
            private PersonSpecification theRecord;

            public WhenImportingMatches()
            {
                identityKind1 = new TestIdentityKind("external-123");
                identityKind2 = new TestIdentityKind("external-444");

                converted = new ImportFileReader(
                        new InlineDataSource(
                            new StudyStub(),
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
                            }),
                        new InMemoryIdentityKindStore(identityKind1, identityKind2))
                    .ToArray();

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