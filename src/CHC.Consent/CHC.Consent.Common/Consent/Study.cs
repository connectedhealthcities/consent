namespace CHC.Consent.Common.Consent
{
    public class Study 
    {
        public StudyIdentity Id { get; protected set; }
        public string Name { get; protected set; }

        /// <inheritdoc />
        public Study(long id, string name=null) : this(name)
        {
            Id = new StudyIdentity(id);
        }

        /// <inheritdoc />
        public Study(string name=null)
        {
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
            return Id.GetHashCode();
        }
    }
}