using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace UnderMineControl.Loader.UI
{
    using Core;
    using Core.Models;

    public class ModInstaller
    {
        private readonly DataTable _mods;
        private readonly ICoreUtility _core;
        private readonly Action<string, int> _statusUpdate;
        private readonly ILogger _logger;
        private Dictionary<string, string[]> _installed;

        private ModInstaller(
            DataTable mods, 
            ICoreUtility core, 
            Action<string, int> statusUpdate,
            ILogger logger)
        {
            _mods = mods;
            _core = core;
            _statusUpdate = statusUpdate;
            _logger = logger;
        }

        public async Task Process()
        {
            _statusUpdate("Sync mod states...", 0);
            _installed = _core.GetInstalledMods() ?? new Dictionary<string, string[]>();

            int total = _mods.Rows.Count,
                progress = 0;

            foreach(DataRow row in _mods.Rows)
            {
                var mod = row.Field<Mod>("mod");
                var installed = row.Field<bool>("Installed");
                int ppb = (int)((double)progress / total * 100);
                progress += 1;
                int ppa = (int)((double)progress / total * 100);

                try
                {
                    var isInstalled = IsInstalled(mod);

                    if (isInstalled == installed)
                    {
                        _statusUpdate($"Skipping {mod.Name}, it doesn't need to be modified.", ppa);
                        continue;
                    }

                    if (installed && !isInstalled)
                    {
                        _statusUpdate($"Installing {mod.Name}...", ppb);
                        var res = await _core.InstallMod(mod.InstallSettings);
                        _statusUpdate(res ?
                            $"Installed {mod.Name}!" :
                            $"Failed to install {mod.Name} - Check output logs!", ppa);
                        continue;
                    }

                    _statusUpdate($"Uninstalling {mod.Name}..", ppb);

                    var ures = _core.UninstallMod(mod.InstallSettings);
                    _statusUpdate(ures ?
                        $"Uninstalled {mod.Name}!" :
                        $"Failed to uninstall {mod.Name} - Check output logs!", ppa);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred while handling mod {mod.Name}");
                    _statusUpdate($"Error occurred while handling: {mod.Name}", ppa);
                }
            }

            _statusUpdate("Finished", 100);
        }

        public bool IsInstalled(Mod mod)
        {
            return IsInstalled(mod.InstallSettings.Owner, mod.InstallSettings.Repo);
        }

        public bool IsInstalled(string owner, string repo)
        {
            return _installed.ContainsKey(owner) &&
                   _installed[owner].Contains(repo);
        }

        public static void ManageInstallChanges(DataTable table, Action<string, int> statusUpdate, ICoreUtility core, ILogger logger)
        {
            new ModInstaller(table, core, statusUpdate, logger)
                .Process()
                .GetAwaiter()
                .GetResult();
        }
    }
}
