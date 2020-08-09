using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace UnderMineControl.Loader.Core.FileManagement
{
    using System.Linq;

    public interface IFileUtility
    {
        bool PurgeCache();
        Task<FileResult> DownloadFile(string url, int maxAgeDays = 30);
    }

    public class FileUtility : IFileUtility
    {
        private const string FILETYPE_CACHE = "Cache";
        private const string FILETYPE_CONFIG = "Config";
        private const string FILETYPE_WORKING = "Working";

        private const string FILE_CACHE = "filecache.json";
        private const string FILE_CACHE_EXT = ".cache";

        private const string FILE_DOWNLOAD_DISPOSITION = "content-disposition";

        private readonly IConfigUtility _config;
        private readonly ILogger _logger;

        private Cache loadedCache;

        public string CacheDirectory => Path.Combine(GetBaseDirectory(), FILETYPE_CACHE);
        public string ConfigDirectory => Path.Combine(GetBaseDirectory(), FILETYPE_CONFIG);
        public string WorkingDirectory => Path.Combine(GetBaseDirectory(), FILETYPE_WORKING);
        public string FileCache => Path.Combine(GetBaseDirectory(), FILE_CACHE);

        public FileUtility(IConfigUtility config, ILogger<FileUtility> logger)
        {
            _config = config;
            _logger = logger;

            EnsureDirectories();
        }

        public void EnsureDirectories()
        {
            var baseDir = GetBaseDirectory();
            var cacheDir = CacheDirectory;
            var configDir = ConfigDirectory;
            var workingDir = WorkingDirectory;

            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);

            if (!Directory.Exists(workingDir))
                Directory.CreateDirectory(workingDir);
        }

        public string GetBaseDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, _config.AppName);
        }

        public string GetFilePath(string filename, FileType type)
        {
            var dir = GetBaseDirectory();

            var sub = FILETYPE_CACHE;
            switch (type)
            {
                case FileType.ConfigFile: sub = FILETYPE_CONFIG; break;
                case FileType.WorkingFile: sub = FILETYPE_WORKING; break;
            }

            var path = Path.Combine(dir, sub, filename);
            if (File.Exists(path))
                return path;

            return null;
        }

        public Cache GetCache()
        {
            try
            {
                var path = FileCache;
                if (loadedCache != null)
                    return loadedCache;

                if (!File.Exists(path))
                    return loadedCache = new Cache();

                var data = File.ReadAllText(path);
                return loadedCache = JsonConvert.DeserializeObject<Cache>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching cache.");
                return loadedCache = new Cache();
            }
        }

        public void SaveCache()
        {
            try
            {
                var data = JsonConvert.SerializeObject(loadedCache);
                File.WriteAllText(FileCache, data);
                _logger.LogDebug("File cache saved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving cache");
            }
        }

        public async Task<FileResult> DownloadFile(string url, int maxAgeDays = 30)
        {
            try
            {
                FileResult result;
                if (maxAgeDays > 0)
                {
                    result = GetCachedFile(url, maxAgeDays);
                    if (result != null && result.Worked)
                    {
                        _logger.LogDebug("Using cached file for: " + url);
                        return result;
                    }
                }

                _logger.LogDebug("Starting download of " + url);

                var guid = Guid.NewGuid().ToString();
                var cachePath = Path.Combine(CacheDirectory, guid + FILE_CACHE_EXT);

                using (var client = new WebClient())
                using (var io = client.OpenRead(url))
                {
                    var filename = ResolveFileName(client, url);

                    using (var sw = File.OpenWrite(cachePath))
                    {
                        await io.CopyToAsync(sw);
                    }

                    result = new FileResult
                    {
                        Cached = new FileCache
                        {
                            Created = DateTime.Now,
                            Guid = guid,
                            OriginalName = filename,
                            WebLocation = url
                        },
                        Code = HttpStatusCode.OK,
                        FileName = filename,
                        Path = cachePath
                    };

                    if (maxAgeDays > 0)
                    {
                        loadedCache.Files.Add(result.Cached);
                        SaveCache();
                    }

                    return result;
                }
            }
            catch (WebException ex)
            {
                _logger.LogError(ex, "WebException error occurred while downloading " + url);
                
                if (ex.Status != WebExceptionStatus.ProtocolError)
                    return new FileResult
                    {
                        Code = HttpStatusCode.InternalServerError,
                        Path = url
                    };

                var statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                return new FileResult
                {
                    Code = statusCode,
                    Path = url
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading and caching a file: " + url);
                return new FileResult
                {
                    Code = HttpStatusCode.InternalServerError,
                    Path = url
                };
            }
        }

        public string ResolveFileName(WebClient client, string url)
        {
            var dispoition = client.ResponseHeaders[FILE_DOWNLOAD_DISPOSITION];
            if (!string.IsNullOrEmpty(dispoition))
                return new ContentDisposition(dispoition).FileName;

            return url.Split('/').Last().Split('?').First();
        }

        public string ValidateCache(FileCache cache, int maxAgeDays)
        {
            if (cache == null)
                return null;

            var maxAge = DateTime.Now.AddDays(-maxAgeDays);
            if (maxAge >= cache.Created)
                return null;

            var filename = cache.Guid + FILE_CACHE_EXT;
            return GetFilePath(filename, FileType.CachedFile);
        }

        public FileResult GetCachedFile(string url, int maxAgeDays)
        {
            var cache = GetCache();
            if (cache.Files.Count <= 0)
                return null;

            foreach(var file in cache.Files.ToArray())
            {
                if (file.WebLocation == url)
                {
                    var filePath = ValidateCache(file, maxAgeDays);
                    if (filePath == null)
                    {
                        loadedCache.Files.Remove(file);
                        SaveCache();
                        return null;
                    }

                    return new FileResult
                    {
                        Code = HttpStatusCode.OK,
                        FileName = file.OriginalName,
                        Cached = file,
                        Path = filePath
                    };
                }
            }

            return null;
        }

        public bool PurgeCache()
        {
            try
            {
                loadedCache = new Cache();
                SaveCache();

                var dir = CacheDirectory;
                if (!Directory.Exists(dir))
                    return true;

                Directory.Delete(dir, true);
                EnsureDirectories();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while trying to purge file cache");
                return false;
            }
        }
    }
}
