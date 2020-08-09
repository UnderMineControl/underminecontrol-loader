using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UnderMineControl.Loader.Core
{
    using FileManagement;
    using Models;

    public interface IModRegistryUtility
    {
        Task<ModRegistry> GetRegistry();
    }

    public class ModRegistryUtility : IModRegistryUtility
    {
        private readonly IFileUtility _file;
        private readonly IConfigUtility _config;
        private readonly ILogger _logger;

        private static ModRegistry _cache = null;

        public ModRegistryUtility(IFileUtility file, IConfigUtility config, ILogger<ModRegistryUtility> logger)
        {
            _file = file;
            _config = config;
            _logger = logger;
        }

        public async Task<ModRegistry> GetRegistry()
        {
            try
            {
                if (_cache != null)
                    return _cache;

                var file = await _file.DownloadFile(_config.ModRegistryUrl, 0);
                if (!file.Worked)
                {
                    _logger.LogWarning("Couldn't download mod registry.");
                    return null;
                }

                var data = File.ReadAllText(file.Path);
                return _cache = JsonConvert.DeserializeObject<ModRegistry>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching mod registry");
                return null;
            }
        }
    }
}
