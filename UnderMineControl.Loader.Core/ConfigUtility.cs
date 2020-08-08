using Microsoft.Extensions.Configuration;

namespace UnderMineControl.Loader.Core.Models
{
    using Setup;

    public interface IConfigUtility
    {
        string AppName { get; }
        string GameName { get; }
        int AppId { get; }
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
        public SettingDependencies Dependencies => _config.Get<SettingDependencies>("Dependencies");
        public Uninstalls UmcUninstall => _config.Get<Uninstalls>("UmcUninstall");
        public Uninstalls VortexUninstall => _config.Get<Uninstalls>("VortexUninstall");

        public ConfigUtility(IConfiguration config)
        {
            _config = config;
        }
    }
}
