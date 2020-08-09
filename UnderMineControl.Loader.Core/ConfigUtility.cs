using Microsoft.Extensions.Configuration;

namespace UnderMineControl.Loader.Core
{
    using Models;
    using Setup;

    public interface IConfigUtility
    {
        string AppName { get; }
        string GameName { get; }
        int AppId { get; }
        string ModRegistryUrl { get; }
        string ModInstallDirectory { get; }
        string UmcInstallDirectory { get; }
        string BepInExConfigLocation { get; }
        SettingDependencies Dependencies { get; }
        Uninstalls UmcUninstall { get; }
        Uninstalls VortexUninstall { get; }
    }

    public class ConfigUtility : IConfigUtility
    {
        private readonly IConfiguration _config;

        public string AppName => _config["AppName"];
        public int AppId => int.Parse(_config["AppId"]);
        public string GameName => _config["GameName"];
        public string ModRegistryUrl => _config["ModRegistryUrl"];
        public string ModInstallDirectory => _config["ModInstallDirectory"];
        public string UmcInstallDirectory => _config["UmcInstallDirectory"];
        public string BepInExConfigLocation => _config["BepInExConfigLocation"];
        public SettingDependencies Dependencies => _config.Get<SettingDependencies>("Dependencies");
        public Uninstalls UmcUninstall => _config.Get<Uninstalls>("UmcUninstall");
        public Uninstalls VortexUninstall => _config.Get<Uninstalls>("VortexUninstall");

        public ConfigUtility(IConfiguration config)
        {
            _config = config;
        }
    }
}
