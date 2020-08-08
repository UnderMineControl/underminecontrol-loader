using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace UnderMineControl.Loader.Cli
{
    using Core;
    using Core.Models;
    using Setup;
    using UnderMineControl.Loader.Core.FileManagement;

    public interface IProgram
    {
        Task Start();
    }

    public class Program : IProgram
    {
        private const string APP_SETTINGS_PATH = "appsettings.json";

        private readonly ILogger _logger;
        private readonly ICoreUtility _core;

        public Program(ILogger<Program> logger, ICoreUtility core)
        {
            _logger = logger;
            _core = core;
        }

        public async Task Start()
        {
            var install = true;

            if (install)
                await Install();
            else
                Uninstall();
        }

        private void Uninstall()
        {
            var worked = _core.Uninstall();
            _logger.LogDebug("Uninstalling worked: " + worked);
        }

        private async Task Install()
        {
            var worked = await _core.InstallUmc();
            _logger.LogDebug("Installing worked: " + worked);

            worked = await _core.InstallModLatest("calico-crusade", "umc-backtravel", "UnderMineControl.BackTravel.zip");
            _logger.LogDebug("Installing backtravel worked: " + worked);
        }

        public static void Main(string[] args)
        {
            try
            {
                //Get our configuration from our settings files
                var config = Settings.Config()
                                     //Load our settings files
                                     .LoadFile(APP_SETTINGS_PATH, false, true)
                                     //Add command line arguments
                                     .AddCommandLine(args)
                                     //Get the config object
                                     .Build();

                //Start our dependency injection setup
                SetupUtility.Start(config)
                            //Add logging
                            .AddLogging()
                            //Register our custom services
                            .AddTransient<IConfigUtility, ConfigUtility>()
                            .AddTransient<ICoreUtility, CoreUtility>()
                            .AddTransient<ISteamUtility, SteamUtility>()
                            .AddTransient<IRepoUtility, RepoUtility>()
                            .AddTransient<IFileUtility, FileUtility>()

                            //Register the main program entry point
                            .AddTransient<IProgram, Program>()
                            //Get our entry point
                            .Build<IProgram>()
                            //Trigger it and wait for the result
                            .Start()
                            .GetAwaiter()
                            .GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while setting up applciation\r\n{0}", ex);
            }
        }
    }
}
