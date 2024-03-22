using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Krialys.Entities.COMMON;

public static class ModelBuilderExtensions
{
    public static void ApplyAllConfigurations<TEntity>(this ModelBuilder modelBuilder) where TEntity : class
    {
        var applyConfigurationMethodInfo = modelBuilder
            .GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(m => m.Name.Equals("ApplyConfiguration", StringComparison.OrdinalIgnoreCase));

        _ = typeof(TEntity).Assembly
            .GetTypes()
            .Select(t => (t, i: t.GetInterfaces().FirstOrDefault(i => i.Name.Equals(typeof(IEntityTypeConfiguration<>).Name, StringComparison.Ordinal))))
            .Where(it => it.i != null)
            .Select(it => (et: it.i.GetGenericArguments()[0], cfgObj: Activator.CreateInstance(it.t)))
            .Select(it => applyConfigurationMethodInfo.MakeGenericMethod(it.et).Invoke(modelBuilder, new[] { it.cfgObj }))
            .ToList();

        // Auto-magically assign Enum as Integer
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var entityBuilder = modelBuilder.Entity(entityType.ClrType);
            foreach (var property in entityType.GetProperties())
            {
                if (typeof(Enum).IsAssignableFrom(property.ClrType))
                    entityBuilder.Property(property.ClrType, property.Name).HasConversion<int>();
            }
        }

        ////Convert all dates to UTC (Read/Write)
        //var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
        //v => v.ToUniversalTime(),
        //(v) => v.Kind == DateTimeKind.Utc ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : TimeZoneInfo.ConvertTimeToUtc(v, TimeZoneInfo.Local));

        //var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
        //  v => v.HasValue ? v.Value.ToUniversalTime() : v,
        //  v => v.HasValue ? v.Value.Kind == DateTimeKind.Utc ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : TimeZoneInfo.ConvertTimeToUtc(v.Value, TimeZoneInfo.Local) : v);

        //foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        //{
        //    if (entityType.IsKeyless) continue;

        //    foreach (var property in entityType.GetProperties())
        //    {
        //        if (property.ClrType == typeof(DateTime)) property.SetValueConverter(dateTimeConverter);
        //        else if (property.ClrType == typeof(DateTime?)) property.SetValueConverter(nullableDateTimeConverter);
        //    }
        //}

    }
}
