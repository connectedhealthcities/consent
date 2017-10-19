using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CHC.Consent.Identity.Core;
using CHC.Consent.Identity.SimpleIdentity;
using CHC.Consent.NHibernate.Consent;
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

        public Configuration(Action<IDbIntegrationConfigurationProperties> setup, Action<string> logHbm=null)
        {
            
            config = new global::NHibernate.Cfg.Configuration();
            var mappingDocument = GetMappings();

            if (logHbm != null)
            {
                var xmlMapping = new StringWriter();
                new XmlSerializer(mappingDocument.GetType()).Serialize(xmlMapping, mappingDocument);
                logHbm(xmlMapping.ToString());
            }

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
                BeforeMapBag += UseNicerFKName;
                BeforeMapSet += UseNicerFKName;

                //AfterMapBag += UseNicerForeignKeyColumnNameForBag;
            }
            
            private void UseNicerFKName(IModelInspector inspector, PropertyPath member, ICollectionPropertiesMapper customizer)
            {
                var propertyType = ((PropertyInfo) member.LocalMember).PropertyType;

                var entityType = propertyType.GetGenericArguments()[0];
                var idMembers = GetIdMembers(inspector, entityType);
                if (idMembers.Length == 1)
                {
                    customizer.Key(key => key.Column(namingStrategy.ClassToTableName(member.LocalMember.DeclaringType.Name) + idMembers[0].Name));
                }
            }


            private void UseNicerForeignKeyNameForManyToOne(IModelInspector inspector, PropertyPath member, IManyToOneMapper customizer)
            {
                var classType = member.LocalMember.DeclaringType;
                customizer.ForeignKey(
                    "FK_" + namingStrategy.ClassToTableName(classType.Name) + "_" +
                    member.ToColumnName("_"));

                var fkType = ((PropertyInfo)member.LocalMember).PropertyType;
                var idMembers = GetIdMembers(inspector, fkType);
                if (idMembers.Length == 1)
                {
                    customizer.Column(namingStrategy.ClassToTableName(fkType.Name) + idMembers[0].Name);
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
                var entityMembersForPoid = MembersProvider.GetEntityMembersForPoid(type).ToArray();
                var idMembers = entityMembersForPoid.Where(inspector.IsPersistentId).ToArray();
                return idMembers;
            }
        }

        private HbmMapping GetMappings()
        {
            var mapper = new ChcConventionsModelMapper();

            var classesToMap = typeof(Entity).Assembly.GetTypes().Where(t => t.IsSubclassOf<Entity>())
                .Concat(new [] {typeof(SimpleIdentity)})
                .ToArray();


            mapper.Class<IdentityKind>(m => { m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid)); });
            mapper.Class<Study>(m => { m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid)); });
            
            mapper.Class<Consent.Consent>(
                m =>
                {
                    m.Set(_ => _.ProvidedEvidence, 
                        e =>
                        {
                            e.Key(k => k.Column("ConsentId"));
                            e.Cascade(Cascade.All | Cascade.DeleteOrphans);
                            e.Table("Consent_ProvidedEvidence");
                            e.Inverse(false);
                        },
                        j => j.ManyToMany());
                    
                    m.Set(_ => _.WithdrawnEvidence, 
                        e =>
                        {
                            e.Key(k => k.Column("ConsentId"));
                            e.Cascade(Cascade.All | Cascade.DeleteOrphans);
                            e.Table("Consent_WithdrawnEvidence");
                            e.Inverse(false);
                        },
                        j => j.ManyToMany());

                    m.HasAcl();

                });
            mapper.Class<Person>(m =>
            {
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

                m.HasAcl();
            });

            

            mapper.Class<Study>(
                m =>
                {
                    m.HasAcl();
                });

            mapper.Class<AccessControlList>(
                m =>
                {
                    m.Set(_ => _.Permissions,
                        c =>
                        {
                            c.Cascade(Cascade.All | Cascade.DeleteOrphans);
                            c.Inverse(false);
                            c.Fetch(CollectionFetchMode.Subselect);
                        });

                    m.Any(_ => _.Owner, typeof(Guid),
                        a =>
                        {
                            a.MetaType<string>();
                            foreach (var securableType in classesToMap.Where(_ => _.IsInheritedFrom<INHibernateSecurable>()))
                            {
                                a.MetaValue(securableType.Name, securableType);
                            }
                        });
                });

            mapper.Class<AccessControlEntry>(
                m =>
                {
                    m.ManyToOne(_ => _.AccessControlList, c => c.NotNullable(true));
                    m.ManyToOne(_ => _.Permisson, c => c.NotNullable(true));
                    m.ManyToOne(_ => _.Principal, c => c.NotNullable(true));
                }
            );
            
            mapper.Class<Identity.Identity>(m =>
            {
                m.ManyToOne(
                    _ => _.Person,
                    j => { j.Index("IX_PersistedIdentity_Person"); });
            });
            
            mapper.Class<SubjectIdentifier>(
                m =>
                {
                    m.Bag(_ => _.Identities,
                        c => c.Key(k => k.NotNullable(true)),
                        j => j.ManyToMany());

                    m.Property(_ => _.StudyId, p => p.NotNullable(true));
                });

            mapper.Class<SecurityPrincipal>(
                m =>
                {
                    m.Abstract(true);
                    m.ManyToOne(_ => _.Role, a => a.NotNullable(false));
                    m.Discriminator(d => d.Column("Type"));
                    m.Set(_ => _.PermissionEntries, c => { c.Cascade(Cascade.All); });
                });
            
            mapper.Class<Authenticatable>(
                m =>
                {
                    m.Abstract(true);
                    m.Set(l => l.Logins, c => { c.Cascade(Cascade.All);});
                });
            
            mapper.Class<Login>(m => m.Discriminator(d => d.Column("Type")));
            mapper.Class<JwtLogin>(m => m.DiscriminatorValue("Jwt"));

            mapper.IsRootEntity((type, b) => IsRootEntity(type));
            mapper.IsTablePerClassHierarchy((type, b) => IsTablePerClass(type));

          
            mapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                if (IsRootEntity(type) && classesToMap.Where(t => t != type).Any(s => s.IsSubclassOf(type)))
                {
                    customizer.Discriminator(d => d.Column("Type"));
                }
            };

            return mapper.CompileMappingFor(classesToMap);
        }

        private static bool IsRootEntity(Type type)
        {
            return type.BaseType == typeof(object) || type.BaseType == typeof(Entity);
        }
        
        public static bool IsTablePerClass(Type t)
        {
            return t.BaseType != typeof(Entity) && t.IsSubclassOf<Entity>();
        }
    }

    public static class AclMappingExtensions
    {
        public static void HasAcl<T>(this IClassMapper<T> m) where T : class, INHibernateSecurable
        {
            m.ManyToOne(_ => _.Acl, c =>
            {
                c.NotNullable(true);
                c.Unique(true);
                c.Update(true);
                c.Insert(true);
                c.Cascade(Cascade.All);
            });
        }
    }
}