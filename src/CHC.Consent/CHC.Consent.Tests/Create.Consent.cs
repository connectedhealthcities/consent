using System;
using System.ComponentModel;
using System.IO;
using CHC.Consent.Common.Consent;

namespace CHC.Consent.Tests
{
    public static partial class Create
    {
        public static StudyBuilder Study => new StudyBuilder();

        public class StudyBuilder : Builder<Study, StudyBuilder>
        {
            private long studyId = Math.Abs(BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0));
            private string name = Guid.NewGuid().ToString();

            public StudyBuilder WithId(long newId) => Copy(_ => _.studyId = newId);
            public StudyBuilder WithName(string newName) => Copy(_ => _.name = newName);            

            /// <inheritdoc />
            public override Study Build()
            {
                return new Study(studyId, name:name);
            }
        }
    }
}