using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnderMineControl.Loader.Core
{
    public interface ISteamUtility
    {
        string FindGameDirectory(string gameName);
        bool Verify(int appId);
    }

    public class SteamUtility : ISteamUtility
    {
        private const string STEAM_REGKEY_32 = @"SOFTWARE\Valve\Steam";
        private const string STEAM_REGKEY_64 = @"SOFTWARE\Wow6432Node\Valve\Steam";
        private const string STEAM_REGKEY_INSTALL = "InstallPath";
        private const string STEAM_DEFAULT_INSTALL = @"C:\Program Files\Steam";
        private const string STEAM_GAME_FOLDER = @"steamapps\common\";
        private const string STEAM_LIBRARYFOLDERS_FILE = @"steamapps\libraryfolders.vdf";
        private const string STEAM_VERIFY = "steam://validate/";

        private readonly ILogger _logger;

        public SteamUtility(ILogger<SteamUtility> logger)
        {
            _logger = logger ?? throw new NullReferenceException("logger");
        }

        public RegistryKey GetSteamRegistryKey(out bool isX64)
        {
            isX64 = false;
            try
            {
                _logger.LogDebug("Checking for steam registry keys");

                var x64 = Registry.LocalMachine.OpenSubKey(STEAM_REGKEY_64);
                if (x64 != null &&
                    !string.IsNullOrEmpty(x64.ToString()))
                {
                    _logger.LogDebug("Found 64 bit steam reg key");
                    isX64 = true;
                    return x64;
                }

                var x32 = Registry.LocalMachine.OpenSubKey(STEAM_REGKEY_32);
                if (x32 != null &&
                    !string.IsNullOrEmpty(x32.ToString()))
                {
                    _logger.LogDebug("Found 32 bit steam reg key");
                    return x32;
                }

                _logger.LogWarning("Couldn't find a steam registry key!");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to find steam registry keys");
                return null;
            }
        }

        public string GetBaseInstallPath()
        {
            try
            {
                _logger.LogDebug("Starting to find Steam Base Install Path");

                var key = GetSteamRegistryKey(out _);

                if (key != null)
                {
                    var regpath = key.GetValue(STEAM_REGKEY_INSTALL)?.ToString();
                    _logger.LogInformation($"Found steam install path: {regpath}");
                    if (!string.IsNullOrEmpty(regpath) && Directory.Exists(regpath))
                        return regpath;

                    _logger.LogWarning($"Steam install path couldn't be verified: {regpath}");
                }

                _logger.LogDebug("Attempting to resolve default steam install path as registry key failed!");
                if (Directory.Exists(STEAM_DEFAULT_INSTALL))
                    return STEAM_DEFAULT_INSTALL;

                _logger.LogWarning("Couldn't find any steam installs, is steam even installed?");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting Steam Install Path");
                return null;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> ParseVdf(string data)
        {
            var lines = data.Split('\n');
            foreach(var line in lines)
            {
                if (!line.StartsWith("\t"))
                    continue;

                var parts = line.Trim('\t').Split(new[] { "\t\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    yield return new KeyValuePair<string, string>(parts[0].Trim('\"'), parts[1].Trim('\"'));
            }
        }

        public IEnumerable<string> GetGameDirectories(string baseInstallPath)
        {
            yield return baseInstallPath;

            var path = Path.Combine(baseInstallPath, STEAM_LIBRARYFOLDERS_FILE);
            if (!File.Exists(path))
            {
                _logger.LogWarning($"library folders VDF file doesn't exist: {path}");
                yield break;
            }

            var data = File.ReadAllText(path);
            var vdf = ParseVdf(data).ToArray();

            foreach (var entry in vdf)
            {
                if (!int.TryParse(entry.Key, out _))
                    continue;

                if (Directory.Exists(entry.Value))
                    yield return entry.Value;
            }
        }

        public bool Verify(int appId)
        {
            try
            {
                _logger.LogInformation("Starting steam file verification...");
                Process.Start(new ProcessStartInfo
                {
                    FileName = STEAM_VERIFY + appId,
                    UseShellExecute = true,
                    CreateNoWindow = false
                });
                _logger.LogInformation("Steam file verification finished!");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying game files");
                return false;
            }
        }

        public string FindGameDirectory(string gameName)
        {
            return @"C:\Users\Cardboard\Desktop\UnderMineTest"; //Temporary testing

            try
            {
                var basePath = GetBaseInstallPath();
                if (basePath == null)
                    return null;

                var gameDirectories = GetGameDirectories(basePath);
                foreach(var dir in gameDirectories)
                {
                    var path = Path.Combine(dir, STEAM_GAME_FOLDER, gameName);
                    if (Directory.Exists(path))
                    {
                        _logger.LogDebug($"{gameName} directory found: {path}");
                        return path;
                    }
                }

                _logger.LogWarning("Unable to find directory for game: " + gameName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while trying to find the game directory.");
                return null;
            }
        }
    }
}
