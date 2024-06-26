﻿using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Krialys.Orkestra.WebApi.Services.Extensions;

public static class DbContextExtensions
{
    public static IQueryable Set(this DbContext context, Type T)
    {
        // Get the generic type definition
        var method = typeof(DbContext).GetMethod(nameof(DbContext.Set), BindingFlags.Public | BindingFlags.Instance);

        // Build a method with the specific type argument you're interested in
        if (method is null) return default;

        method = method.MakeGenericMethod(T);

        return method.Invoke(context, null) as IQueryable;
    }

    public static IQueryable<T> Set<T>(this DbContext context)
    {
        // Get the generic type definition
        var method = typeof(DbContext).GetMethod(nameof(DbContext.Set), BindingFlags.Public | BindingFlags.Instance);

        // Build a method with the specific type argument you're interested in
        if (method is null) return default;

        method = method.MakeGenericMethod(typeof(T));

        return method.Invoke(context, null) as IQueryable<T>;
    }
}