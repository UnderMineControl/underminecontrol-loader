using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace UnderMineControl.Loader.Core
{
    using Models;

    public interface IUninstallUtility
    {
        bool Uninstall();
        bool Uninstall(Uninstalls uninstalls);
        bool UninstallMod(string owner, string repo);
    }

    public class UninstallUtility : IUninstallUtility
    {
        private readonly IConfigUtility _config;
        private readonly ISteamUtility _steam;
        private readonly ILogger _logger;

        public UninstallUtility(
            IConfigUtility config,
            ISteamUtility steam,
            ILogger<CoreUtility> logger)
        {
            _config = config;
            _steam = steam;
            _logger = logger;
        }

        public bool Uninstall()
        {
            Uninstall(_config.UmcUninstall);
            return _steam.Verify(_config.AppId);
        }

        public bool Uninstall(Uninstalls uninstalls)
        {
            var gameDirectory = _steam.FindGameDirectory(_config.GameName);
            if (string.IsNullOrEmpty(gameDirectory))
            {
                _logger.LogWarning("Could not find game directory!");
                return false;
            }

            foreach (var file in uninstalls.Files)
            {
                var fullPath = Path.Combine(gameDirectory, file);
                FindAndDelete(fullPath);
            }

            foreach (var dir in uninstalls.Directories)
            {
                var fullpath = Path.Combine(gameDirectory, dir);
                _logger.LogDebug($"Deleting directory: {fullpath}");
                if (Directory.Exists(fullpath))
                    Directory.Delete(fullpath, true);
            }

            return true;
        }

        public bool UninstallMod(string owner, string repo)
        {
            var gameDirectory = _steam.FindGameDirectory(_config.GameName);
            if (string.IsNullOrEmpty(gameDirectory))
            {
                _logger.LogWarning("Could not find game directory!");
                return false;
            }

            var modsDir = Path.Combine(gameDirectory, _config.ModInstallDirectory, owner, repo);
            if (!Directory.Exists(modsDir))
                return true;

            Directory.Delete(modsDir, true);
            _logger.LogDebug("Deleting directory: " + modsDir);
            return true;
        }

        public bool FindAndDelete(string filePath)
        {
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileName(filePath);

                if (!Directory.Exists(dir))
                {
                    _logger.LogDebug("Directory doesn't exist, no files to delete: " + dir);
                    return true;
                }

                var files = Directory.GetFiles(dir, fileName);
                if (files.Length == 0)
                    return true;

                foreach (var file in files)
                {
                    _logger.LogDebug($"Deleting file: {file}");

                    File.Delete(file);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while attempting to delete file(s): " + filePath);
                return false;
            }
        }
    }
}
