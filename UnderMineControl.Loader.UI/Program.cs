using System;
using System.Windows.Forms;

namespace UnderMineControl.Loader.UI
{
    using Core;
    using Core.FileManagement;
    using Setup;

    public static class Program
    {
        private const string APP_SETTINGS_PATH = "appsettings.json";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var instance = DependencyInjection();
            Application.Run(instance);
        }

        public static frmMods DependencyInjection()
        {
            //Get our configuration from our settings files
            var config = Settings.Config()
                                 //Load our settings files
                                 .LoadFile(APP_SETTINGS_PATH, false, true)
                                 //Get the config object
                                 .Build();

            //Start our dependency injection setup
            return SetupUtility.Start(config)
                        //Add logging
                        .AddLogging()
                        //Register our custom services
                        .AddTransient<IConfigUtility, ConfigUtility>()
                        .AddTransient<ICoreUtility, CoreUtility>()
                        .AddTransient<ISteamUtility, SteamUtility>()
                        .AddTransient<IRepoUtility, RepoUtility>()
                        .AddTransient<IFileUtility, FileUtility>()
                        .AddTransient<IModRegistryUtility, ModRegistryUtility>()
                        .AddTransient<IInstallUtility, InstallUtility>()
                        .AddTransient<IUninstallUtility, UninstallUtility>()

                        .AddSingleton<frmMods>()

                        .Build<frmMods>();
        }

        public static void InvokeAction(this Control ctrl, Action action)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke((MethodInvoker)delegate { action(); });
            else
                action();
        }
    }
}
