using System;
using System.Collections.Generic;
using System.IO;

namespace CHC.Consent.Common.Import.Datasources
{
    public class FileDatasource : IStandardDataDatasource
    {
        public string FileLocation { get; }

        public FileDatasource(string fileLocation)
        {
            FileLocation = fileLocation;
        }

        public IEnumerable<IPerson> People => throw new NotImplementedException();

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