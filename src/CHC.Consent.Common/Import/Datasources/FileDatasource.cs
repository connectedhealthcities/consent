using System.Collections.Generic;
using System.IO;
using CHC.Consent.Common.Core;

namespace CHC.Consent.Common.Import.Datasources
{
    public class FileDatasource : IStandardDataDatasource
    {
        public string FileLocation { get; }
        public IStudy Study { get; }


        public FileDatasource(string fileLocation, IStudy study)
        {
            FileLocation = fileLocation;
            Study = study;
        }

        public IEnumerable<IImportRecord> People => CreateStandardDataReader().People();

        public StandardDataReader CreateStandardDataReader()
        {
            var fileExtension = Path.GetExtension(FileLocation);
            if (fileExtension == ".xml")
            {
                return new XmlStandardDataReader(this);
            }
            throw new FileTypeNotSupportedException(fileExtension);
        }
    }
}