namespace CHC.Consent.Security
{
    public interface IJwtIdentifiedUserRepostiory
    {
        IUser FindUserBy(string issuer, string subject);
    }
}