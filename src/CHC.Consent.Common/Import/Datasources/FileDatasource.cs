namespace CHC.Consent.Common.Import.Datasources
{
    public class FileDatasource : IStandardDataDatasource
    {
        public string FileLocation { get; }

        public FileDatasource(string fileLocation)
        {
            FileLocation = fileLocation;
        }
    }
}