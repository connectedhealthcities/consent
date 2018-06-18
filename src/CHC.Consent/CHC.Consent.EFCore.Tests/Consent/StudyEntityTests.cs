using System.Linq;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.Testing.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests.Consent
{
    [Collection(DatabaseCollection.Name)]
    public class StudyEntityTests : DbTests
    {
        /// <inheritdoc />
        public StudyEntityTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
        }

        [Fact]
        public void CreatesANewAclForAStudy()
        {
            var study = createContext.Add(new StudyEntity { Name = Random.String() }).Entity;
            createContext.SaveChanges();

            var studyEntity = readContext.Set<StudyEntity>().Include(_ => _.ACL).Single(_ => _.Id == study.Id);
            
            Assert.NotNull(studyEntity.ACL);
            Assert.Equal($"Study {study.Id}", studyEntity.ACL.Description);
        }
    }
}