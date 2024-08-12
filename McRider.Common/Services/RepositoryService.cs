using McRider.Common.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace McRider.Common.Services;

public class RepositoryService<T>
{
    FileCacheService _fileCacheService;
    MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    string _fileName;

    public RepositoryService(FileCacheService fileCacheService, string? fileName = null)
    {
        _fileCacheService = fileCacheService;
        _fileName = fileName;
    }

    private static string FilePrefix => FileCacheService.FilePrefix;

    public string FileName
    {
        get
        {
            var fileName = _fileName ?? $"{typeof(T).Name.ToLower()}s.json".Replace("ss.json", "s.json");

            if (fileName.EndsWith("configs.json"))
                return fileName;

            return FilePrefix + fileName;
        }
    }

    public async Task<List<T>> GetAllAsync()
    {
        if (_memoryCache.TryGetValue(FileName, out var cachedData) && cachedData is List<T> cachedTData)
            return cachedTData;

        return await _fileCacheService.GetAsync<List<T>>(FileName, () => Task.FromResult(new List<T>()));
    }

    public async Task<T?> GetAsync(string id)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(x => id?.Equals(x?.GetFirstValue("Id", "_id")) == true);
    }

    public async Task<T[]> Find(Func<T, bool>? predicate = null, int page = 1, int pageSize = 10, params string[] sortBy)
    {
        predicate ??= new Func<T, bool>((a) => true);

        var all = await GetAllAsync();
        var filtered = all.Where(predicate);
        var sorted = filtered.OrderBy(x => 0);

        foreach (var sort in sortBy)
        {
            if (!string.IsNullOrEmpty(sort))
            {
                var sortby = sort.Split(':').FirstOrDefault();
                var dir = sort.Split(':').LastOrDefault()?.ToLower() == "desc" ? -1 : 1;
                sorted = dir == 1 ? sorted.ThenBy(x => x?.GetFirstValue(sort)) : sorted.ThenByDescending(x => x?.GetFirstValue(sort));
            }
        }

        var pagenated = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        return pagenated;
    }

    public async Task<T[]> Find(Func<T, bool> predicate, params string[] sortBy) => await Find(predicate, 1, 100, sortBy);

    public async Task<T> SaveAsync(T item)
    {
        var all = await GetAllAsync();
        var id = item?.GetFirstValue("Id", "_id");


        if (string.IsNullOrEmpty(id?.ToString()))
            throw new ArgumentException("Id is required to save an item.");

        item?.SetValue("ModifiedDate", DateTime.UtcNow);
        var indexOf = all.FindIndex(x => id?.Equals(x?.GetFirstValue("Id", "_id")) == true);
        if (indexOf >= 0)
            all[indexOf] = item;
        else
            all.Add(item);

        _memoryCache.Set(FileName, all, TimeSpan.FromMinutes(60));
        await _fileCacheService.SetAsync(FileName, all);

        return item;
    }

    public async Task<IEnumerable<T>> SaveAllAsync(IEnumerable<T> list)
    {
        var all = (await GetAllAsync()) ?? [];

        foreach (var item in list)
        {
            var id = item?.GetFirstValue("Id", "_id");

            if (string.IsNullOrEmpty(id?.ToString()))
                throw new ArgumentException("Id is required to save an item.");

            item?.SetValue("ModifiedDate", DateTime.UtcNow);
            var indexOf = all.FindIndex(x => id?.Equals(x?.GetFirstValue("Id", "_id")) == true);
            if (indexOf >= 0)
                all[indexOf] = item;
            else
                all.Add(item);
        }

        _memoryCache.Set(FileName, list, TimeSpan.FromMinutes(60));
        await _fileCacheService.SetAsync(FileName, all);

        return all;
    }

    public async Task<int> Delete(Func<T, bool> predicate)
    {
        var all = await GetAllAsync();
        var count = 0;

        foreach (var item in all.Where(x => predicate(x)).ToArray())
            if (all.Remove(item)) count++;

        _memoryCache.Set(FileName, all, TimeSpan.FromMinutes(60));
        await _fileCacheService.SetAsync(FileName, all);

        return count;
    }
}

public static class RepositoryExtensions
{
    public async static Task<T> Save<T>(this T obj)
    {
        if (obj == null) return obj;
        var repository = new RepositoryService<T>(new FileCacheService());
        return await repository.SaveAsync(obj);
    }

    public async static Task<IEnumerable<T>> SaveAll<T>(this IEnumerable<T> list)
    {
        if (list == null) return list;
        var repository = new RepositoryService<T>(new FileCacheService());
        return await repository.SaveAllAsync(list);
    }
}