using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class Permisson : Entity, IPermisson
    {
        /// <inheritdoc />
        public virtual string Name { get; set; }

        public const string Read = "read";
        public const string Write = "write";
    }
}