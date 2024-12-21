
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using ServerApp.Contracts.Repositories;
using ServerApp.Domain.Data;
using System.Linq.Expressions;
using System.Text.Json;

namespace ServerApp.Persistence.Base;
public abstract class RepositoryBase<T, TContext> : IRepositoryBase<T, TContext> 
    where T : class
    where TContext : DbContext
{
    protected TContext _appDbContext { get; set; }

    private readonly IDistributedCache _cache;
    private readonly TimeSpan _cacheExpiration;
    private readonly bool useCache;

    public RepositoryBase(TContext context, IDistributedCache cache, IConfiguration config)
    {
        _appDbContext = context;
        _cache = cache;
        _cacheExpiration = TimeSpan.FromMinutes(int.Parse(config["CacheSettings:DurationMn"]??"60"));
        useCache = config["CacheSettings:Active"] == "1";
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        string cacheKey = $"GetAll_{typeof(T).Name}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (useCache && !string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<IEnumerable<T>>(cachedData);
        }

        var data = await _appDbContext.Set<T>().AsNoTracking().ToListAsync();
        var serializedData = JsonSerializer.Serialize(data);

        await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        });

        return data;
    }

    public async Task<T?> FindAsync(int id)
    {
        string cacheKey = $"Find_{typeof(T).Name}_{id}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (useCache && !string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<T>(cachedData);
        }

        var data = await _appDbContext.Set<T>().FindAsync(id);
        if (data != null)
        {
            var serializedData = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            });
        }

        return data;
    }

    public async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IQueryable<T>>? includeProperties = null)
    {
        string cacheKey = $"GetByCondition_{typeof(T).Name}_{expression.ToString()}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (useCache && !string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<IEnumerable<T>>(cachedData);
        }

        IQueryable<T> query = _appDbContext.Set<T>().Where(expression).AsNoTracking();

        if (includeProperties != null)
        {
            query = includeProperties(query);
        }

        var data = await query.ToListAsync();
        var serializedData = JsonSerializer.Serialize(data);

        await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        });

        return data;
    }

    public async Task CreateAsync(T entity)
    {
        await _appDbContext.Set<T>().AddAsync(entity);

        // Invalidate the cache for GetAllAsync
        string cacheKey = $"GetAll_{typeof(T).Name}";
        await _cache.RemoveAsync(cacheKey);
    }

    public async Task UpdateAsync(T entity)
    {
        _appDbContext.Set<T>().Update(entity);
        await Task.CompletedTask;

        // Invalidate the cache for GetAllAsync
        string cacheKey = $"GetAll_{typeof(T).Name}";
        await _cache.RemoveAsync(cacheKey);

        string entityCacheKey = $"Find_{typeof(T).Name}_{GetEntityId(entity)}";
        await _cache.RemoveAsync(entityCacheKey);
    }

    public async Task DeleteAsync(T entity)
    {
        _appDbContext.Set<T>().Remove(entity);
        await Task.CompletedTask;

        // Invalidate the cache for GetAllAsync
        string cacheKey = $"GetAll_{typeof(T).Name}";
        await _cache.RemoveAsync(cacheKey);

        string entityCacheKey = $"Find_{typeof(T).Name}_{GetEntityId(entity)}";
        await _cache.RemoveAsync(entityCacheKey);
    }

    public async Task<bool> Exists(int id)
    {
        return await FindAsync(id) != null;
    }

    public async Task SaveAsync() => await _appDbContext.SaveChangesAsync();

    private int GetEntityId(T entity)
    {
        var property = typeof(T).GetProperty("Id");
        if (property == null)
        {
            throw new InvalidOperationException("Entity does not have an 'Id' property.");
        }

        return (int)property.GetValue(entity)!;
    }
}