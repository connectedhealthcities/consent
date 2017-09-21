using System;
using System.Runtime.InteropServices.ComTypes;
using CHC.Consent.Common.Identity;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NHibernate.SqlTypes;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Type;

namespace CHC.Consent.NHibernate
{
    public class IdentityConfiguration
    {
        public ISession Session { get; }

        public IdentityConfiguration(Action<string> scriptAction)
        {
            
            var config = new Configuration();
            config
                .DataBaseIntegration(db =>
                {
                    db.Dialect<MsSql2012Dialect>();
                    db.Driver<SqlServer2008Driver>();
                    db.ConnectionString = "Data Source=.;Initial Catalog=CHC;Integrated Security=True;";
                    db.ConnectionReleaseMode = ConnectionReleaseMode.OnClose;
                    db.LogFormattedSql = true;
                    db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;                    
                })
                .AddMapping(GetMappings());
            var sessionFactory = config.BuildSessionFactory();
            Session = sessionFactory.OpenSession();


            var schemaExport = new SchemaExport(config);
            schemaExport.Execute(
                scriptAction,
                execute: true,
                justDrop: true);
            
            schemaExport.Create(scriptAction, execute: true);
            
                    
            
            Session.Clear();
        }


        class ChcConventionsModelMapper : ConventionModelMapper
        {
            readonly INamingStrategy namingStrategy = DefaultNamingStrategy.Instance;
            public ChcConventionsModelMapper():base()
            {
                BeforeMapClass += UseNativeGenerator;
                BeforeMapManyToOne += UseNicerForeignKeyNameForManyToOne;
            }

            private void UseNicerForeignKeyNameForManyToOne(IModelInspector inspector, PropertyPath member, IManyToOneMapper customizer)
            {
                customizer.ForeignKey(
                    "FK_" + namingStrategy.ClassToTableName(member.LocalMember.ReflectedType.Name) + "_" +
                    member.ToColumnName("_"));
            }

            private void UseNativeGenerator(IModelInspector inspector, Type type, IClassAttributesMapper customizer)
            {
                customizer.Id(id => id.Generator(Generators.Native));
            }
            
            
        }
        
        private HbmMapping GetMappings()
        {
            var mapper = new ChcConventionsModelMapper();
            
            mapper.Class<IdentityKind>(m =>
            {
                m.Id(_ => _.Id, id => id.Generator(Generators.Assigned));
               
            });
            mapper.Class<Identity>(m => m.Discriminator(d => {d.Type(NHibernateUtil.Character);}));
            mapper.Class<SimpleIdentity>(m => { m.DiscriminatorValue('s'); });
            mapper.Class<CompositeIdentity>(m => m.DiscriminatorValue('c'));

            mapper.IsTablePerClassHierarchy((s, b) => typeof(Identity).IsAssignableFrom(s));
            mapper.IsTablePerClassSplit((s,b) => false);
            mapper.IsTablePerClass((type, b) => typeof(Identity).IsAssignableFrom(type));
            mapper.IsRootEntity((type, b) => !typeof(Identity).IsAssignableFrom(type) || typeof(Identity) == type);
            
            return mapper.CompileMappingFor(
                new[] {typeof(IdentityKind), typeof(Identity), typeof(SimpleIdentity), typeof(CompositeIdentity)});

            /*
             
             var mapper = new ModelMapper();
             
             mapper.AddMapping<IdentityKindMapping>();
             mapper.AddMapping<IdentityMapping>();
             mapper.AddMapping<SimpleIdentityMapping>();
             mapper.AddMapping<ComplexIdentityMapping>();
 
             var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
             
             return mapping;*/
        }
    }
}