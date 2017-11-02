using System;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Import.Datasources;
using CHC.Consent.Security;
using Xunit;

namespace CHC.Consent.Common.Tests.Import.Datasources
{
    public class FileDatasourceTests
    {
        private class Study:IStudy {
            /// <inheritdoc />
            public Guid Id { get; } = Guid.NewGuid();

            /// <inheritdoc />
            public IAccessControlList AccessControlList => throw new NotImplementedException();
        }
        
        readonly IStudy study = new Study();
        
        [Fact]
        public void CreatesXmlDatasourceForXmlFiles()
        {
            var datasource = new FileDatasource("ignored.xml", study);

            Assert.IsType<XmlStandardDataReader>(datasource.CreateStandardDataReader());
        }

        [Fact]
        public void CannotCreateDatasourceForUnknownFiles()
        {
            Assert.Throws<FileTypeNotSupportedException>(
                () => new FileDatasource("unhandled.xyz", study).CreateStandardDataReader());
        }
    }
}