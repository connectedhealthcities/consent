namespace CHC.Consent.Security
{
    public interface IRole : ISecurityPrincipal
    { 
        string Description { get; }
        string Name { get; }
    }
}