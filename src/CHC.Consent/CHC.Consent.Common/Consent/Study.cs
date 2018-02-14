namespace CHC.Consent.Common.Consent
{
    public class Study
    {
        public string Id { get; }
        public string Name { get; protected set; }

        /// <inheritdoc />
        public Study(string id, string name=null)
        {
            Id = id;
            Name = name;
        }

        protected bool Equals(Study other)
        {
            return string.Equals(Id, other.Id);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Study) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}