namespace CHC.Consent.Security
{
    public interface IJwtIdentifiedUserRepository
    {
        IAuthenticatable FindUserBy(string issuer, string subject);
    }
}