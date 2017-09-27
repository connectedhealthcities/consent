using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.Common.Import
{
    public class ImportFileReader : IEnumerable<PersonSpecification>
    {
        private readonly IStandardDataDatasource source;
        private readonly IIdentityKindStore identityKinds;

        public ImportFileReader(IStandardDataDatasource source, IIdentityKindStore identityKinds)
        {
            this.source = source;
            this.identityKinds = identityKinds;
        }


        public IEnumerator<PersonSpecification> GetEnumerator()
        {
            return new ImportFileEnumerator(source, identityKinds);
        }

        public class ImportFileEnumerator : IEnumerator<PersonSpecification>
        {
            private IEnumerator<IImportRecord> sourceEnumerator;
            public IStandardDataDatasource Source { get; }
            public IIdentityKindStore IdentityKinds { get; }

            public ImportFileEnumerator(IStandardDataDatasource source, IIdentityKindStore identityKinds)
            {
                Source = source;
                sourceEnumerator = source.People.GetEnumerator();
                IdentityKinds = identityKinds;
            }

            public bool MoveNext()
            {
                if (!sourceEnumerator.MoveNext())
                {
                    Current = null;
                    return false;
                }

                Current = Convert(sourceEnumerator.Current);
                return true;
            }

            public PersonSpecification Convert(IImportRecord import)
            {
                return new PersonSpecification
                {
                    
                };
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public PersonSpecification Current { get; set; }
            object IEnumerator.Current => Current;
            public void Dispose()
            {
                sourceEnumerator.Dispose();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}