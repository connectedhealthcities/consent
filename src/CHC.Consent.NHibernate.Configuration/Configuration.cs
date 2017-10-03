using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CHC.Consent.Identity.Core;
using CHC.Consent.Identity.SimpleIdentity;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.Utils;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.Loquacious;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

namespace CHC.Consent.NHibernate.Configuration
{
    public class Configuration : ISessionFactory
    {
        private readonly global::NHibernate.ISessionFactory sessionFactory;
        private readonly global::NHibernate.Cfg.Configuration config;

        public static Action<IDbIntegrationConfigurationProperties> SqlServer(string connectionString)
            =>
                db =>
                {
                    db.Dialect<MsSql2012Dialect>();
                    db.Driver<SqlServer2008Driver>();
                    db.ConnectionString = connectionString;
                };

        

        public Configuration(Action<IDbIntegrationConfigurationProperties> setup)
        {
            
            config = new global::NHibernate.Cfg.Configuration();
            var mappingDocument = GetMappings();

            var xmlMapping = new StringWriter();
            new XmlSerializer(mappingDocument.GetType()).Serialize((TextWriter) xmlMapping, (object) mappingDocument);
            
            config.DataBaseIntegration(db =>
                {
                    db.LogFormattedSql = true;
                    db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                    setup(db);
                })
                .AddMapping(mappingDocument);
            sessionFactory = config.BuildSessionFactory();
        }

        public void DropSchema(Action<string> output=null, bool execute=false)
        {
            var schemaExport = new SchemaExport(config);
            schemaExport.Execute(
                output ?? Do.Nothing,
                execute,
                justDrop: true);
        }

        public void Create(Action<string> output = null, bool execute = false)
        {
            var schemaExport = new SchemaExport(config);
            schemaExport.Create(output ?? Do.Nothing, true);
        }

        public ISession StartSession()
        {
            var session = sessionFactory.OpenSession();
            session.FlushMode = FlushMode.Commit;
            return session;
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
                var idMembers = MembersProvider.GetEntityMembersForPoid(type).Where(inspector.IsPersistentId).ToArray();
                if (idMembers.Length > 1) return;
                
                var idMember = idMembers.FirstOrDefault();
                if (idMember == null) return;

                if (idMember is FieldInfo fieldInfo && fieldInfo.FieldType == typeof(Guid))
                {
                    customizer.Id(id => id.Generator(Generators.Guid));
                    return;
                }

                if (idMember is PropertyInfo propertyInfo && propertyInfo.PropertyType == typeof(Guid))
                {
                    customizer.Id(id => id.Generator(Generators.Guid));
                    return;
                }

                customizer.Id(id => id.Generator(Generators.Native));
            }
            
            
        }

        private HbmMapping GetMappings()
        {
            var mapper = new ChcConventionsModelMapper();

            mapper.Class<IdentityKind>(m => { m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid)); });
            mapper.Class<Consent.Study>(m => { m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid)); });
            
            mapper.Class<Consent.Consent>(
                m =>
                {
                    m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid));
                    m.Bag(_ => _.ProvidedEvidence, 
                        e =>
                        {
                            e.Cascade(Cascade.All | Cascade.DeleteOrphans);
                            e.Table("Consent_ProvidedEvidence");
                            e.Inverse(false);
                        },
                        j => j.ManyToMany());
                    
                    m.Bag(_ => _.WithdrawnEvidence, 
                        e =>
                        {
                            e.Cascade(Cascade.All | Cascade.DeleteOrphans);
                            e.Table("Consent_WithdrawnEvidence");
                            e.Inverse(false);
                        },
                        j => j.ManyToMany());
                    
                });
            mapper.Class<Consent.Evidence>(m => { m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid)); });
            mapper.Class<Consent.EvidenceKind>(m => { m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid)); });
            mapper.Class<PersistedPerson>(m =>
            {
                m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid));
                m.Bag(
                    _ => _.Identities,
                    j =>
                    {
                        j.Cascade(Cascade.Persist);
                        j.Inverse(true);
                    });
                m.Bag(
                    _ => _.SubjectIdentifiers,
                    j =>
                    {
                        j.Cascade(Cascade.Persist);
                        j.Inverse(true);
                    });
            });
            
            mapper.Class<PersistedIdentity>(m =>
            {
                m.ManyToOne(
                    _ => _.Person,
                    j => { j.Index("IX_PersistedIdentity_Person"); });
            });
            
            mapper.Class<PersistedSubjectIdentifier>(
                m =>
                {
                    m.Bag(_ => _.Identities,
                        Do.Nothing,
                        j =>
                        {
                            j.ManyToMany();
                        });
                });
            
            mapper.IsTablePerClassHierarchy((s, b) => typeof(PersistedIdentity).IsAssignableFrom(s));
            
            
            return mapper.CompileMappingFor(
                new[]
                {
                    typeof(PersistedPerson),
                    typeof(IdentityKind), 
                    typeof(PersistedIdentity), 
                    typeof(PersistedSimpleIdentity), 
                    typeof(PersistedSubjectIdentifier),
                    typeof(Consent.Study),
                    typeof(Consent.Consent),
                    typeof(Consent.Evidence),
                    typeof(Consent.EvidenceKind)
                });
        }
    }
}