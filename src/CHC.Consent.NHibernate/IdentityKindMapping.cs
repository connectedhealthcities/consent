using System;
using System.Data.Common;
using System.Xml.Linq;
using CHC.Consent.Common.Identity;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace CHC.Consent.NHibernate
{
    public class IdentityKindMapping : ClassMapping<IdentityKind>
    {
        public IdentityKindMapping()
        {
            Id(
                _ => _.Id,
                m =>
                {
                    m.Generator(Generators.Assigned);
                    m.Length(255);
                }
            );
            Property(_ => _.Format, p => p.Type(NHibernateUtil.Int32));
        }
    }

    public class IdentityMapping : ClassMapping<Common.Identity.Identity>
    {
        public IdentityMapping()
        {
            Table("`Identity`");
            Id(_ => _.Id, m => m.Generator(Generators.Native));
            ManyToOne(_ => _.IdentityKind, m => { m.NotNullable(true); });
        }
    }

    public class SimpleIdentityMapping : SubclassMapping<SimpleIdentity>
    {
        public SimpleIdentityMapping()
        {
            DiscriminatorValue('S');
            Property(_ => _.Value, m => { m.Length(int.MaxValue);});
        }
    }

    public class ComplexIdentityMapping : SubclassMapping<CompositeIdentity>
    {
        public ComplexIdentityMapping()
        {
            DiscriminatorValue('C');
            Property(_ => _.CompositeValue);
        }
    }
}