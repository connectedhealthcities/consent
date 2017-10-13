using System;
using CHC.Consent.Testing.NHibernate;
using Xunit;

namespace CHC.Consent.NHibernate.Tests
{
    using Study = NHibernate.Consent.Study;
    [Collection(DatabaseCollection.Name)]
    public class StudyTest
    {
        private readonly DatabaseFixture db;

        public StudyTest(DatabaseFixture db)
        {
            this.db = db;
        }

        [Fact]
        public void TestStudyPersistence()
        {
            object id = null;
            
            db.InTransactionalUnitOfWork(session => { id = session.Save(new Study()); });

            db.InTransactionalUnitOfWork(
                session =>
                {
                    var study = session.Get<Study>(id);

                    Assert.NotEqual(Guid.Empty, study.Id);
                }
            );
        }
    }
}