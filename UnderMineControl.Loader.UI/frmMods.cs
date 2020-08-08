using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnderMineControl.Loader.UI
{
    using Controls;
    using Core;
    using Properties;

    public partial class frmMods : Form
    {
        private readonly ICoreUtility _coreUtility;

        public frmMods()
        {
            //_coreUtility = new CoreUtility();
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            menuMain.Build()
                    .AddSubMenu("File", _ =>
                    {
                        _.AddSubMenu("Install", c =>
                         {
                             c.AddButton("Latest", () => MenuButton_InstallUMC(true), "Installs the latest version of UnderMineControl")
                              .AddButton("Specific Version", () => MenuButton_InstallUMC(false), "Installs a specific version of UnderMineControl");
                         })
                         .AddButton("Uninstall UMC", MenuButton_UninstallUMC, "Uninstalls UnderMineControl and deletes all installed mods!")
                         .AddSeperator()
                         .AddButton("Disable All Mods", MenuButton_DisableMods, "Disables all mods currently installed");
                    })
                    .AddButton("Visit Github", MenuButton_VisitGithub, "Opens a browser and directs you to the UMC github page");
        }

        private void MenuButton_VisitGithub()
        {
            var url = Settings.Default.GithubUrl;
            _coreUtility.OpenWebPage(url);
        }

        private void MenuButton_InstallUMC(bool latest = true)
        {

        }

        private void MenuButton_UninstallUMC()
        {

        }

        private void MenuButton_DisableMods()
        {

        }
    }
}
