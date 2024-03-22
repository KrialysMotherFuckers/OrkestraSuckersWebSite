using Krialys.Orkestra.WebApi.Services.Common.Factories;
using Krialys.Orkestra.WebApi.Services.Data;
using Krialys.Orkestra.WebApi.Services.System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Orkestra.WebApi.Services.Common;

/// <summary>
/// Implements IGenericCRUDService thanks to open types, means 1 service for all entities
/// </summary>
public class DbmsServices<TEntity, TContext> : GenericCrud<TEntity, TContext>
    where TEntity : class
    where TContext : DbContext
{
    public DbmsServices(TContext dbContext, ITrackedEntitiesServices trackedEntitiesServices,
        IHttpContextAccessor httpContextAccessor, Serilog.ILogger logger, ISqlRaw sqlRaw, ISqliteQueryFactory queryFactory,
        EtiquettesServices etiquettesServices)
        : base(dbContext, trackedEntitiesServices, httpContextAccessor, logger, sqlRaw, queryFactory, etiquettesServices) { }
}