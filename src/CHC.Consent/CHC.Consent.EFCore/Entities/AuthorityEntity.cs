using CHC.Consent.Common.Identity;

namespace CHC.Consent.EFCore.Entities
{
    public class AuthorityEntity : IEntity
    {
        /// <inheritdoc />
        public long Id { get; protected set; }
        
        public string Name { get; protected set; }
        
        public string SystemName { get; protected set; }
        
        public int Priority { get; protected set; }
        
        protected  AuthorityEntity()
        {
        }
        
        /// <inheritdoc />
        public AuthorityEntity(string name, int priority, string systemName)
        {
            Name = name;
            Priority = priority;
            SystemName = systemName;
        }

        public Authority ToAuthority()
        {
            return new Authority
            {
                Id = (AuthorityIdentity) Id,
                Name = Name,
                Priority = Priority,
                SystemName = SystemName
            };
        }
    }
}