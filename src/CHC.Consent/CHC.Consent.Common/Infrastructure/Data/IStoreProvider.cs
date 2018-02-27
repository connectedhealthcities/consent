namespace CHC.Consent.Common.Infrastructure.Data
{
    public interface IStoreProvider
    {
        IStore<T> Get<T>() where T : class, IEntity;
    }
}