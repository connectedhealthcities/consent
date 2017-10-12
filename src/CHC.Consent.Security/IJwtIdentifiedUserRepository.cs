namespace CHC.Consent.Security
{
    public interface IJwtIdentifiedUserRepository
    {
        IUser FindUserBy(string issuer, string subject);
    }
}