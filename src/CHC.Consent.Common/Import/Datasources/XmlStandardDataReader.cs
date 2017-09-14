namespace CHC.Consent.Common.Import.Datasources
{
    public class XmlStandardDataReader : StandardDataReader
    {
        private readonly FileDatasource datasource;

        public XmlStandardDataReader(FileDatasource datasource)
        {
            this.datasource = datasource;
        }
    }
}