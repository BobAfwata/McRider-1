using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace McRider.Common.Services;

public class FileCacheService
{
    private ILogger _logger;
    private readonly string _cacheDirectory;
    private readonly JsonSerializerSettings settings;
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 2);

    public TimeSpan? DefaultCacheDuration { get; set; }

    public FileCacheService(ILogger<FileCacheService>? logger = null)
    {
        _logger = logger;
        var currentAssemblyName = string.Join(".", (Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "files.").Split('.').SkipLast(1));
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            currentAssemblyName
        );

        if (!Directory.Exists(_cacheDirectory))
            Directory.CreateDirectory(_cacheDirectory);

        settings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects, // Preserve object references
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize, // Handle reference loops
            Formatting = Formatting.Indented // Optional, for pretty-printing JSON
        };
    }

    public string Get(string key, TimeSpan? cacheDuration = null)
    {
        string filePath = GetCacheFilePath(key);
        if (!File.Exists(filePath))
            return null; // Cache does not exist


        try
        {
            semaphore.Wait();

            cacheDuration ??= DefaultCacheDuration;
            if (cacheDuration != null)
            {
                var cacheExpiry = File.GetLastWriteTime(filePath).Add(cacheDuration.Value);
                if (DateTime.UtcNow > cacheExpiry)
                    return null; // Cache has expired
            }

            return File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error while reading file: {0}", filePath);
            return null;
        }
        finally
        {
            semaphore.Release(); // Release the semaphore
        }
    }

    public void Set(string key, string data)
    {
        string filePath = GetCacheFilePath(key);
        try
        {
            semaphore.Wait();
            File.WriteAllText(filePath, data);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error while writing to cache file: {0}", filePath);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task SetAsync<T>(string key, T data)
    {
        string filePath = GetCacheFilePath(key);
        try
        {
            await semaphore.WaitAsync();
            await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, settings));
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error while writing to cache file: {0}", filePath);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public T Get<T>(string key, Func<T> readFunc, TimeSpan? cacheDuration = null)
    {
        string cachedData = Get(key, cacheDuration);

        if (string.IsNullOrEmpty(cachedData))
        {
            T freshData = readFunc();

            _ = SetAsync(key, freshData);

            return freshData;
        }

        return JsonConvert.DeserializeObject<T>(cachedData, settings);
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

        return JsonConvert.DeserializeObject<T>(cachedData, settings);
    }

    public void Remove(string key)
    {
        string filePath = GetCacheFilePath(key);
        try
        {
            semaphore.Wait();
            if (File.Exists(filePath)) File.Delete(filePath);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error while deleting cache file: {0}", filePath);
        }
        finally
        {
            semaphore.Release();
        }

    }

    private string GetCacheFilePath(string key)
    {
        return Path.Combine(_cacheDirectory, key + ".json").Replace(".json.json", ".json").Replace("ss.json", "s.json");
    }
}