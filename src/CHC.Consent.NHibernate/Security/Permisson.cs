using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public abstract class Permisson : Entity, IPermisson
    {
        /// <inheritdoc />
        public virtual string Name { get; set; }
    }
}