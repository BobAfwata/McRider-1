using Newtonsoft.Json;
using System.Reflection;

namespace McRider.Common.Services;

public class FileCacheService
{
    private readonly string _cacheDirectory;

    public FileCacheService()
    {
        var currentAssemblyName = string.Join(".", (Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "files.").Split(',').SkipLast(1));
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            currentAssemblyName, "Cache"
        );

        if (!Directory.Exists(_cacheDirectory))
            Directory.CreateDirectory(_cacheDirectory);
    }

    public string Get(string key, TimeSpan? cacheDuration = null)
    {
        string filePath = Path.Combine(_cacheDirectory, key + ".json");
        if (File.Exists(filePath))
        {
            var cacheExpiry = File.GetLastWriteTime(filePath).Add(cacheDuration ?? TimeSpan.FromMinutes(60)); // Set cache expiry to 30 minutes by default
            if (DateTime.UtcNow > cacheExpiry)
                return null;

            return File.ReadAllText(filePath);
        }

        return null;
    }

    public void Set(string key, string data)
    {
        string filePath = Path.Combine(_cacheDirectory, key + ".json");
        File.WriteAllText(filePath, data);
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

            Set(key, JsonConvert.SerializeObject(freshData));

            return freshData;
        }

        return JsonConvert.DeserializeObject<T>(cachedData);
    }

    public void Remove(string key)
    {
        string filePath = Path.Combine(_cacheDirectory, key);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}