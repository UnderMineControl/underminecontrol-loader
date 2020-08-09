using Ionic.Zip;
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
    using Models;

    public interface IInstallUtility
    {
        Task<bool> InstallMod(Dependency dependency);
        Task<bool> InstallUmc();
        Dictionary<string, string[]> InstalledMods();
    }

    public class InstallUtility : IInstallUtility
    {
        private readonly IRepoUtility _repo;
        private readonly IConfigUtility _config;
        private readonly ISteamUtility _steam;
        private readonly ILogger _logger;
        private readonly IFileUtility _file;
        private readonly IUninstallUtility _uninstall;

        public InstallUtility(
            IConfigUtility config,
            ISteamUtility steam,
            IRepoUtility repo,
            ILogger<InstallUtility> logger,
            IFileUtility file,
            IUninstallUtility uninstall)
        {
            _config = config;
            _repo = repo;
            _steam = steam;
            _logger = logger;
            _file = file;
            _uninstall = uninstall;
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

            var mid = Path.Combine(gameDirectory, _config.ModInstallDirectory);
            if (!Directory.Exists(mid))
            {
                Directory.CreateDirectory(mid);
                File.WriteAllText(Path.Combine(mid, "PLEASE DO NOT TOUCH.txt"), 
                    @"These folders are managed by UnderMineControl loader. 
Please don't modify them or their containing data unless you know what you're doing! 
Thanks, 
Cardboard (calico-crusade)");
            }

            var modsDir = Path.Combine(gameDirectory, _config.ModInstallDirectory, dependency.Owner, dependency.Repo);
            if (!Directory.Exists(modsDir))
                Directory.CreateDirectory(modsDir);

            foreach (var file in modFiles)
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

        public async Task<bool> InstallUmc()
        {
            _uninstall.Uninstall(_config.VortexUninstall);

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

            var bepPluginsDir = Path.Combine(gameDirectory, _config.UmcInstallDirectory);
            if (!Directory.Exists(bepPluginsDir))
                Directory.CreateDirectory(bepPluginsDir);

            _logger.LogDebug($"Extracting UnderMineControl files to {bepPluginsDir}");

            foreach (var file in umcFiles)
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

        public Dictionary<string, string[]> InstalledMods()
        {
            var mods = new Dictionary<string, string[]>();

            var gameDirectory = _steam.FindGameDirectory(_config.GameName);
            if (string.IsNullOrEmpty(gameDirectory))
            {
                _logger.LogWarning("Could not find game directory!");
                return null;
            }

            var modsDirectory = _config.ModInstallDirectory;
            var root = Path.Combine(gameDirectory, modsDirectory);

            if (!Directory.Exists(root))
            {
                _logger.LogWarning("Could not find automatic mod install directory");
                return null;
            }

            var dirs = Directory.GetDirectories(root);
            foreach(var dir in dirs)
            {
                var ownersMods = new List<string>();
                var owner = Path.GetFileName(dir.Trim(Path.DirectorySeparatorChar));
                var repos = Directory.GetDirectories(dir);
                foreach(var repo in repos)
                {
                    var repoName = Path.GetFileName(repo.Trim(Path.DirectorySeparatorChar));
                    ownersMods.Add(repoName);
                }

                mods.Add(owner, ownersMods.ToArray());
            }

            return mods;
        }
    }
}
