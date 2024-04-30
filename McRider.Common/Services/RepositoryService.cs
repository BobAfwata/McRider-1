using McRider.Common.Extensions;

namespace McRider.Common.Services;

public class RepositoryService<T>
{
    FileCacheService _fileCacheService;

    public RepositoryService(FileCacheService fileCacheService)
    {
        _fileCacheService = fileCacheService;
    }

    public string FileName => $"{typeof(T).Name.ToLower()}s.json".Replace("ss.json", "s.json");

    public async Task<T[]> GetAllAsync()
    {
        return await _fileCacheService.GetAsync<T[]>(FileName, () => Task.FromResult(Array.Empty<T>()));
    }

    public async Task<T?> GetAsync(string id)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(x => x.GetFirstValue("Id", "_id") == id);
    }

    public async Task<T[]> Find(Func<T, bool> predicate, int page = 1, int pageSize = 10, params string[] sortBy)
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
        var list = all.ToList();
        var id = item.GetFirstValue("Id", "_id");

        if (string.IsNullOrEmpty(id?.ToString()))
            throw new ArgumentException("Id is required to save an item.");

        var indexOf = list.FindIndex(x => x?.GetFirstValue("Id", "_id") == id);
        if (indexOf >= 0)
            list[indexOf] = item;
        else
            list.Add(item);

        await _fileCacheService.SetAsync(FileName, list);

        return item;
    }

    public async Task<IEnumerable<T>> SaveAllAsync(IEnumerable<T> list)
    {
        var alllist = (await GetAllAsync())?.ToList() ?? [];

        foreach (var item in list)
        {
            var id = item?.GetFirstValue("Id", "_id");

            if (string.IsNullOrEmpty(id?.ToString()))
                throw new ArgumentException("Id is required to save an item.");

            var indexOf = alllist.FindIndex(x => x?.GetFirstValue("Id", "_id") == id);
            if (indexOf >= 0)
                alllist[indexOf] = item;
            else
                alllist.Add(item);
        }

        await _fileCacheService.SetAsync(FileName, alllist);

        return alllist;
    }
}

public static class RepositoryExtensions
{
    public async static Task<T> Save<T>(this T obj)
    {
        var repository = new RepositoryService<T>(new FileCacheService());
        return await repository.SaveAsync(obj);
    }

    public async static Task<IEnumerable<T>> SaveAll<T>(this IEnumerable<T> list)
    {
        var repository = new RepositoryService<T>(new FileCacheService());
        return await repository.SaveAllAsync(list);
    }
}