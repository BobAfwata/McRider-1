using Newtonsoft.Json;
using System.Reflection;

namespace McRider.Common.Services;

public class FileCacheService
{
    private readonly string _cacheDirectory;
    public TimeSpan? DefaultCacheDuration { get; set; }

    public FileCacheService()
    {
        var currentAssemblyName = string.Join(".", (Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "files.").Split('.').SkipLast(1));
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            currentAssemblyName
        );

        if (!Directory.Exists(_cacheDirectory))
            Directory.CreateDirectory(_cacheDirectory);
    }

    public string Get(string key, TimeSpan? cacheDuration = null)
    {
        string filePath = GetCacheFilePath(key);
        if (File.Exists(filePath))
        {
            cacheDuration ??= DefaultCacheDuration;
            if (cacheDuration == null)
                return File.ReadAllText(filePath);

            var cacheExpiry = File.GetLastWriteTime(filePath).Add(cacheDuration.Value);
            if (DateTime.UtcNow > cacheExpiry)
                return null;

            return File.ReadAllText(filePath);
        }

        return null;
    }

    public void Set(string key, string data)
    {
        string filePath = GetCacheFilePath(key);
        File.WriteAllText(filePath, data);
    }

    public async Task SetAsync<T>(string key, T data)
    {
        string filePath = GetCacheFilePath(key);
        await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
    }

    public T Get<T>(string key, Func<T> readFunc, TimeSpan? cacheDuration = null)
    {
        string cachedData = Get(key, cacheDuration);

        if (string.IsNullOrEmpty(cachedData))
        {
            T freshData = readFunc();

            Set(key, JsonConvert.SerializeObject(freshData));

            return freshData;
        }

        return JsonConvert.DeserializeObject<T>(cachedData);
    }

    public async Task<T> GetAsync<T>(string key, Func<Task<T>> readFunc, TimeSpan? cacheDuration = null)
    {
        string cachedData = Get(key, cacheDuration);

        if (string.IsNullOrEmpty(cachedData))
        {
            T freshData = await readFunc();

            await SetAsync(key, freshData);

            return freshData;
        }

        return JsonConvert.DeserializeObject<T>(cachedData);
    }

    public void Remove(string key)
    {
        string filePath = GetCacheFilePath(key);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    private string GetCacheFilePath(string key)
    {
        return Path.Combine(_cacheDirectory, key + ".json").Replace(".json.json", ".json").Replace("ss.json", "s.json");
    }
}