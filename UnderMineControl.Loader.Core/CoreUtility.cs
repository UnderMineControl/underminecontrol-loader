using Microsoft.Extensions.Logging;
using Salaros.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace UnderMineControl.Loader.Core
{
    using Models;
    using System.Globalization;

    public interface ICoreUtility
    {
        void OpenWebPage(string url);
        Task<bool> InstallUmc();
        Task<bool> InstallMod(Dependency dependency);
        Task<bool> InstallMod(string owner, string repo, string version, params string[] filenames);
        Task<bool> InstallModLatest(string owner, string repo, params string[] filenames);
        bool Uninstall();
        bool UninstallMod(Dependency dependency);
        bool UninstallMod(string owner, string repo);
        Dictionary<string, string[]> GetInstalledMods();
        bool? IsConsoleEnabled();
        bool ToggleConsole(bool toggle);
        bool VerifyGameFiles();
        void OpenGameFiles();
    }

    public class CoreUtility : ICoreUtility
    {
        private readonly IUninstallUtility _uninstall;
        private readonly IInstallUtility _install;
        private readonly IConfigUtility _config;
        private readonly ISteamUtility _steam;
        private readonly ILogger _logger;

        public CoreUtility(
            IUninstallUtility uninstall, 
            IInstallUtility install, 
            IConfigUtility config, 
            ISteamUtility steam,
            ILogger<CoreUtility> logger)
        {
            _uninstall = uninstall;
            _install = install;
            _config = config;
            _steam = steam;
            _logger = logger;
        }

        public void OpenWebPage(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
                CreateNoWindow = true
            });
        }

        public bool Uninstall()
        {
            return _uninstall.Uninstall();
        }

        public Task<bool> InstallUmc()
        {
            return _install.InstallUmc();
        }

        public Task<bool> InstallMod(Dependency dependency)
        {
            return _install.InstallMod(dependency);
        }

        public Task<bool> InstallModLatest(string owner, string repo, params string[] filenames)
        {
            return InstallMod(new Dependency
            {
                Repo = repo,
                Owner = owner,
                Files = filenames,
                Version = null
            });
        }

        public Task<bool> InstallMod(string owner, string repo, string version, params string[] filenames)
        {
            return InstallMod(new Dependency
            {
                Repo = repo,
                Owner = owner,
                Files = filenames,
                Version = version
            });
        }

        public bool UninstallMod(Dependency dependency)
        {
            return UninstallMod(dependency.Owner, dependency.Repo);
        }

        public bool UninstallMod(string owner, string repo)
        {
            return _uninstall.UninstallMod(owner, repo);
        }

        public Dictionary<string, string[]> GetInstalledMods()
        {
            return _install.InstalledMods();
        }

        public bool? IsConsoleEnabled()
        {
            try
            {
                var cfg = GetParser(out _);
                if (cfg == null)
                    return null;

                var consoleValue = cfg["Logging.Console"]["Enabled"];

                if (consoleValue == null ||
                    !bool.TryParse(consoleValue, out bool enabled))
                    return null;

                return enabled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading BepInEx config file");
                return null;
            }
        }

        public bool ToggleConsole(bool toggle)
        {
            try
            {
                var cfg = GetParser(out string filepath);
                if (cfg == null)
                    return false;

                var consoleValue = cfg["Logging.Console"]["Enabled"];

                cfg.SetValue("Logging.Console", "Enabled", toggle);

                return cfg.Save(filepath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading BepInEx config file");
                return false;
            }
        }

        public ConfigParser GetParser(out string filepath)
        {
            filepath = null;
            var gameDirectory = _steam.FindGameDirectory(_config.GameName);
            if (string.IsNullOrEmpty(gameDirectory))
            {
                _logger.LogWarning("Could not find game directory!");
                return null;
            }

            var configFile = Path.Combine(gameDirectory, _config.BepInExConfigLocation);
            if (!File.Exists(configFile))
            {
                _logger.LogWarning("BepInEx config file doesn't exist. Please run the game first.");
                return null;
            }

            var data = File.ReadAllText(configFile);

            filepath = configFile;
            return new ConfigParser(data, new ConfigParserSettings
            {
                MultiLineValues = MultiLineValues.AllowValuelessKeys,
                Culture = new CultureInfo("en-US")
            });
        }

        public bool VerifyGameFiles()
        {
            return _steam.Verify(_config.AppId);
        }

        public void OpenGameFiles()
        {
            var gameDirectory = _steam.FindGameDirectory(_config.GameName);
            if (string.IsNullOrEmpty(gameDirectory))
            {
                _logger.LogWarning("Could not find game directory!");
                return;
            }

            OpenWebPage(gameDirectory);
        }
    }
}
