using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UnderMineControl.Loader.Core
{
    using FileManagement;
    using Ionic.Zip;
    using Models;

    public interface ICoreUtility
    {
        void OpenWebPage(string url);
        Task<bool> InstallUmc();
        Task<bool> InstallMod(Dependency dependency);
        Task<bool> InstallMod(string owner, string repo, string version, params string[] filenames);
        Task<bool> InstallModLatest(string owner, string repo, params string[] filenames);
        bool Uninstall();
    }

    public class CoreUtility : ICoreUtility
    {
        private const string UMC_INSTALL_DIR = "BepInEx\\plugins\\UnderMineControl";
        private const string UMC_MODS_DIR = "Mods";

        private readonly IRepoUtility _repo;
        private readonly IConfigUtility _config;
        private readonly ISteamUtility _steam;
        private readonly ILogger _logger;
        private readonly IFileUtility _file;

        public CoreUtility(
            IConfigUtility config, 
            ISteamUtility steam, 
            IRepoUtility repo, 
            ILogger<CoreUtility> logger, 
            IFileUtility file)
        {
            _config = config;
            _repo = repo;
            _steam = steam;
            _logger = logger;
            _file = file;
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

        public Task<FileResult[]> GetFiles(Dependency dependency)
        {
            return GetFiles(dependency.Owner, dependency.Repo, dependency.Version, dependency.Files.Select(t => (WildCard)t).ToArray());
        }

        public Task<FileResult[]> GetLatestFiles(string owner, string repo, params WildCard[] filenames)
        {
            return GetFiles(owner, repo, null, filenames);
        }

        public async Task<FileResult[]> GetFiles(string owner, string repo, string version, params WildCard[] fileNames)
        {
            var release = await _repo.GetRelease(owner, repo, version);
            version = version ?? "latest";
            if (release == null)
            {
                _logger.LogWarning($"Could not find a release of {owner}/{repo} version {version}");
                return null;
            }

            var fileRes = new List<FileResult>();

            var requiredAssets = release.Assets
                                        .Where(t => fileNames.Any(a => a == t.Name))
                                        .ToArray();

            if (requiredAssets.Length <= 0)
            {
                _logger.LogWarning($"None of the requested files are assets of {owner}/{repo} version {version}. Request Files: {string.Join(", ", fileNames.Select(t => t.ToString()))}");
                return null;
            }

            return await ForEachAsync(requiredAssets, t => _file.DownloadFile(t.BrowserDownloadUrl));
        }

        public Task<TOut[]> ForEachAsync<TIn, TOut>(IEnumerable<TIn> data, Func<TIn, Task<TOut>> action)
        {
            return Task.WhenAll(data.Select(action));
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

        public async Task<bool> InstallUmc()
        {
            Uninstall(_config.VortexUninstall);

            var umcFiles = await GetFiles(_config.Dependencies.UnderMineControl);
            if (umcFiles == null || umcFiles.Length <= 0 ||
                umcFiles.Any(t => !t.Worked))
            {
                _logger.LogWarning("Could not find any UMC files");
                return false;
            }

            var bep = (await GetFiles(_config.Dependencies.BepInEx)).FirstOrDefault();
            if (bep == null || !bep.Worked)
            {
                _logger.LogWarning("Could not find the BepInEx file!");
                return false;
            }

            var gameDirectory = _steam.FindGameDirectory(_config.GameName);
            if (string.IsNullOrEmpty(gameDirectory))
            {
                _logger.LogWarning("Could not find game directory!");
                return false;
            }

            _logger.LogDebug($"Extracting BepInEx files to: {gameDirectory}");

            using (var zip = ZipFile.Read(bep.Path))
            {
                zip.ExtractAll(gameDirectory, ExtractExistingFileAction.OverwriteSilently);
            }

            _logger.LogDebug("Finished extracting BepInEx files!");

            var bepPluginsDir = Path.Combine(gameDirectory, UMC_INSTALL_DIR);
            if (!Directory.Exists(bepPluginsDir))
                Directory.CreateDirectory(bepPluginsDir);

            _logger.LogDebug($"Extracting UnderMineControl files to {bepPluginsDir}");

            foreach(var file in umcFiles)
            {
                var fileName = Path.Combine(bepPluginsDir, file.FileName);
                File.Copy(file.Path, fileName, true);
                _logger.LogDebug($"Copied {file.Path} to {fileName}");
            }

            _logger.LogDebug($"UMC files copied.");

            var modsDir = Path.Combine(gameDirectory, "Mods");
            if (!Directory.Exists(modsDir))
                Directory.CreateDirectory(modsDir);

            return true;
        }

        public async Task<bool> InstallMod(Dependency dependency)
        {
            var modFiles = await GetFiles(dependency);
            if (modFiles == null || modFiles.Length <= 0 ||
                modFiles.Any(t => !t.Worked))
            {
                _logger.LogWarning("Could not find mod files.");
                return false;
            }

            var gameDirectory = _steam.FindGameDirectory(_config.GameName);
            if (string.IsNullOrEmpty(gameDirectory))
            {
                _logger.LogWarning("Could not find game directory!");
                return false;
            }

            var modsDir = Path.Combine(gameDirectory, UMC_MODS_DIR, dependency.Owner, dependency.Repo);
            if (!Directory.Exists(modsDir))
                Directory.CreateDirectory(modsDir);

            foreach(var file in modFiles)
            {
                var ext = Path.GetExtension(file.FileName).ToLower().Trim('.');
                if (ext == "zip")
                {
                    _logger.LogDebug($"Extracting {dependency.Owner}/{dependency.Repo}'s mod to {modsDir}");
                    using (var zip = ZipFile.Read(file.Path))
                        zip.ExtractAll(modsDir, ExtractExistingFileAction.OverwriteSilently);
                    _logger.LogDebug("Extract complete");
                    continue;
                }

                var fileName = Path.Combine(modsDir, file.FileName);
                File.Copy(file.Path, fileName);
                _logger.LogDebug($"Copied {file.Path} to {fileName}");
            }

            return true;
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
    }
}
