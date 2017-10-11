using System;
using NHibernate;

namespace CHC.Consent.NHibernate
{
    public abstract class Entity
    {
        public virtual Guid Id { get; protected set; }
 
        public override bool Equals(object obj)
        {
            var compareTo = obj as Entity;
 
            if (ReferenceEquals(compareTo, null))
                return false;
 
            if (ReferenceEquals(this, compareTo))
                return true;
 
            if (GetRealType() != compareTo.GetRealType())
                return false;
 
            if (!IsTransient() && !compareTo.IsTransient() && Id == compareTo.Id)
                return true;
 
            return HasSameBusinessValueAs(compareTo);
        }

        protected virtual bool HasSameBusinessValueAs(Entity compareTo) => false;

        public static bool operator ==(Entity a, Entity b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
 
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
 
            return a.Equals(b);
        }
 
        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }
 
        public override int GetHashCode()
        {
            return (GetRealType().ToString() + Id).GetHashCode();
        }
 
        public virtual bool IsTransient()
        {
            return Id == default(Guid);
        }
 
        public virtual Type GetRealType()
        {
            return NHibernateUtil.GetClass(this);
        }
    }
}