using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using CHC.Consent.Identity.Core;
using CHC.Consent.Identity.SimpleIdentity;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.NHibernate.Security;
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
                BeforeMapBag += UseNicerForeignKeyColumnNameForBag;
                AfterMapBag += UseNicerForeignKeyColumnNameForBag;
            }

            private void UseNicerForeignKeyColumnNameForBag(IModelInspector inspector, PropertyPath member, IBagPropertiesMapper customizer)
            {
                var idMembers = GetIdMembers(inspector, ((PropertyInfo)member.LocalMember).PropertyType);
                if (idMembers.Length == 1)
                {
                    customizer.Key(key => key.Column(member.ToColumnName("_") + idMembers[0].Name));
                }
            }


            private void UseNicerForeignKeyNameForManyToOne(IModelInspector inspector, PropertyPath member, IManyToOneMapper customizer)
            {
                var classType = member.LocalMember.ReflectedType;
                customizer.ForeignKey(
                    "FK_" + namingStrategy.ClassToTableName(classType.Name) + "_" +
                    member.ToColumnName("_"));
                
                var idMembers = GetIdMembers(inspector, ((PropertyInfo)member.LocalMember).PropertyType);
                if (idMembers.Length == 1)
                {
                    customizer.Column(member.ToColumnName("_") + idMembers[0].Name);
                }
            }

            private void UseNativeGenerator(IModelInspector inspector, Type type, IClassAttributesMapper customizer)
            {
                var idMembers = GetIdMembers(inspector, type);
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

            private MemberInfo[] GetIdMembers(IModelInspector inspector, Type type)
            {
                var idMembers = MembersProvider.GetEntityMembersForPoid(type).Where(inspector.IsPersistentId).ToArray();
                return idMembers;
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
                            e.Key(k => k.Column("ConsentId"));
                            e.Cascade(Cascade.All | Cascade.DeleteOrphans);
                            e.Table("Consent_ProvidedEvidence");
                            e.Inverse(false);
                        },
                        j => j.ManyToMany());
                    
                    m.Bag(_ => _.WithdrawnEvidence, 
                        e =>
                        {
                            e.Key(k => k.Column("ConsentId"));
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
                        j.Key(k => k.Column("PersonId"));
                        j.Cascade(Cascade.Persist);
                        j.Inverse(true);
                    });
                m.Bag(
                    _ => _.SubjectIdentifiers,
                    j =>
                    {
                        j.Key(k => k.Column("PersonId"));
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
                        c =>
                        {
                            c.Key(k => k.NotNullable(true));
                        },
                        j =>
                        {
                            j.ManyToMany();
                        });
                });

            mapper.Class<SecurityPrincipal>(
                m =>
                {
                    m.Abstract(true);
                    m.ManyToOne(_ => _.Role, a => a.NotNullable(false));
                    m.Discriminator(d => d.Column("Type"));
                });
            
            mapper.Class<Authenticatable>(
                m =>
                {
                    m.Abstract(true);
                    m.Set(l => l.Logins, c => { c.Cascade(Cascade.All);});
                });
            
            mapper.Class<JwtLogin>(m => m.DiscriminatorValue("Jwt"));
            
            mapper.IsRootEntity((type, b) => type.BaseType == typeof(object) || type.BaseType == typeof(Entity));
            mapper.IsTablePerClassHierarchy((type, b) => type.IsInheritedFrom<PersistedIdentity>() || IsTablePerClass(type));

            mapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                if (type.BaseType == typeof(Entity))
                {
                    customizer.Discriminator(d => d.Column("Type"));
                }
            };

            return mapper.CompileMappingFor(
                typeof(Entity).Assembly.GetTypes().Where(t => t.IsSubclassOf<Entity>())
                    .Concat(
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
                            typeof(Consent.EvidenceKind),

                        })
                    .Distinct()
            );
        }

        public static bool IsTablePerClass(Type t)
        {
            return t.BaseType != typeof(Entity) && t.IsSubclassOf<Entity>();
        }

        private static void MapGuidSet<TEntity>(
            ICollectionPropertiesContainerMapper<TEntity> m, Expression<Func<TEntity, IEnumerable<Guid>>> property,
            string tableName, string valueColumn, string keyColumnId) where TEntity : class
        {

            void CollectionMapping(ISetPropertiesMapper<TEntity, Guid> c)
            {
                c.Table(tableName);
                c.Cascade(Cascade.All);
                c.Key(k => k.Column(keyColumnId));
            }

            m.Set(
                property,
                CollectionMapping,
                r => r.Element(
                    e =>
                    {
                        e.Type(NHibernateUtil.Guid);
                        e.Column(
                            c =>
                            {
                                c.Index($"IX_{tableName}_{valueColumn}");
                                c.Name(valueColumn);
                            });
                        e.NotNullable(true);
                    }));
        }
    }
}