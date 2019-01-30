using CHC.Consent.Common.Infrastructure.Definitions;

namespace CHC.Consent.EFCore
{
    ///<summary>
    ///Common properties for classes storing implementations of <see cref="IDefinition"/>
    /// </summary>
    public interface IDefinitionEntity
    {
        string Name { get; set; }
        string Definition { get; set; }
    }
}