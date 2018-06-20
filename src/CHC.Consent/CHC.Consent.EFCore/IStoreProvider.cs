namespace CHC.Consent.EFCore
{
    public interface IStoreProvider
    {
        IStore<T> Get<T>() where T : class, IEntity;
    }
}