﻿using System;
using System.Xml.Serialization;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Utils;
using CHC.Consent.NHibernate.Identity;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.Loquacious;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

namespace CHC.Consent.NHibernate
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
            config.DataBaseIntegration(db =>
                {
                    db.LogFormattedSql = true;
                    db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                    setup(db);
                })
                .AddMapping(GetMappings());
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
                customizer.Id(id => id.Generator(Generators.Native));
            }
            
            
        }

        private HbmMapping GetMappings()
        {
            var mapper = new ChcConventionsModelMapper();

            mapper.Class<IdentityKind>(m => { m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid)); });
            mapper.Class<Study>(m => { m.Id(_ => _.Id, id => id.Generator(Generators.NativeGuid)); });
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
            });
            
            mapper.Class<PersistedIdentity>(m =>
            {
                m.ManyToOne(
                    _ => _.Person,
                    j => { j.Index("IX_PersistedIdentity_Person"); });
            });
            
            mapper.IsTablePerClassHierarchy((s, b) => typeof(PersistedIdentity).IsAssignableFrom(s));
            
            return mapper.CompileMappingFor(
                new[]
                {
                    typeof(PersistedPerson),
                    typeof(IdentityKind), 
                    typeof(PersistedIdentity), 
                    typeof(PersistedSimpleIdentity), 
                    typeof(Study)
                });
        }
    }
}