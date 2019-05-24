using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.EFCore
{
    public static class ModelBuilderExtensions
    {
        private static readonly MethodInfo ApplyConfigurationMethodInfo =
            typeof(ModelBuilder).GetMethods()
                .Where(
                    m => m.Name == nameof(ModelBuilder.ApplyConfiguration) && m.ContainsGenericParameters &&
                         m.GetGenericArguments().Length == 1)
                .Select(m => (m, Parameters: m.GetParameters()))
                .Where(m => m.Parameters.Length == 1)
                .Select(m => (m.m, m.Parameters[0].ParameterType))
                .Where(
                    m => (m.ParameterType.IsConstructedGenericType) &&
                         (m.ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                .Select(m => m.m)
                .First();

        public static ModelBuilder ApplyConfiguration<TConfiguration>(this ModelBuilder builder)
            where TConfiguration: new()
        {
            var configuration = new TConfiguration();
            foreach (var @interface in typeof(TConfiguration).GetInterfaces())
            {
                if(!@interface.IsConstructedGenericType) continue;
                if(@interface.GetGenericTypeDefinition() != typeof(IEntityTypeConfiguration<>)) continue;

                var entityType = @interface.GetGenericArguments()[0];
                ApplyConfigurationMethodInfo.MakeGenericMethod(entityType)
                    .Invoke(builder, new object[] {configuration});
            }

            return builder;
        }

    }
}