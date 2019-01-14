namespace CHC.Consent.EFCore.Security
{
    public abstract class Securable : ISecurable
    {
        protected Securable(string description)
        {
            ACL = new AccessControlList { Description = description};
        }

        /// <inheritdoc />
        public AccessControlList ACL { get; private set; }
    }
}