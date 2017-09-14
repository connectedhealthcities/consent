using System;

namespace CHC.Consent.Common.Import.Datasources
{
    public class FileTypeNotSupportedException : Exception
    {
        public FileTypeNotSupportedException(string fileExtension) : base($"'{fileExtension}' files are not supported")
        {
        }
    }
}