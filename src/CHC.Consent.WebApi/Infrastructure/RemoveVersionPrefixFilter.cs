using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.WebApi.Infrastructure
{
    public class RemoveVersionPrefixFilter : IOperationFilter
    {
        private static readonly int VersionPrefixLength = VersionPrefix.Length;
        private const string VersionPrefix = "V{version";

        /// <inheritdoc />
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.OperationId = operation.OperationId.StartsWith(VersionPrefix)
                ? operation.OperationId.Substring(VersionPrefixLength)
                : operation.OperationId;
        }
    }
}