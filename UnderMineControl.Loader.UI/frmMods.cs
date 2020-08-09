using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace UnderMineControl.Loader.UI
{
    using Controls;
    using Core;
    using Core.FileManagement;
    using Core.Models;
    using System.IO;

    public partial class frmMods : Form
    {
        private readonly ICoreUtility _core;
        private readonly IModRegistryUtility _registry;
        private readonly IFileUtility _file;
        private readonly ILogger _logger;

        private DataTable _mods;

        public frmMods(ICoreUtility core,
            IModRegistryUtility registry, 
            IFileUtility file,
            ILogger<frmMods> logger)
        {
            _core = core;
            _registry = registry;
            _file = file;
            _logger = logger;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            menuMain.Build()
                    .AddSubMenu("Install", _ =>
                    {
                        _.AddButton("Latest", () => MenuButton_InstallUMC(true), "Installs the latest version of UnderMineControl")
                         .AddButton("Version...", () => MenuButton_InstallUMC(false), "Installs a specific version of UnderMineControl");
                    })
                    .AddSubMenu("Uninstall", _ =>
                    {
                        _.AddButton("UnderMineControl", MenuButton_UninstallUMC, "Uninstalls UnerMindControl and all mods")
                         .AddButton("All Mods", MenuButton_DisableMods, "Uninstalls all currently installed mods");
                    })
                    .AddSubMenu("Options", _ =>
                    {
                        _.AddButton("Purge File Cache", MenuButton_PurgeFileCache, "Purges all cached files, these will be redownloaded again if needed.")
                         .AddButton("Console", MenuButton_EnableConsole, "Toggles whether or not to show the game console (helpful for debugging)", _ =>
                         {
                             _.Text = (_core.IsConsoleEnabled() ?? false) ? "Disable Console" : "Enable Console";
                         })
                         .AddButton("Verify Files", MenuButton_VerifyFiles, "Causes steam to verify UnderMine game files to see if anything needs to be reinstalled")
                         .AddButton("View Logs", MenuButton_ViewLogs, "Opens the directory for the loaders logs")
                         .AddButton("View Game Files", _core.OpenGameFiles, "Opens the UnderMine game files directory");
                    })
                    .AddSubMenu("Visit...", _ =>
                    {
                        _.AddButton("UMC Github", 
                            () => MenuButton_VisitPage("https://github.com/calico-crusade/underminecontrol"), 
                            "Opens a browser and directs you to the UMC github page")
                         .AddButton("Developer Documentation",
                            () => MenuButton_VisitPage("https://github.com/calico-crusade/underminecontrol/wiki/Mod-Development"),
                            "Takes you to the UMC developer guides wiki")
                         .AddButton("Get Help (UnderMine discord)",
                            () => MenuButton_VisitPage("https://discord.gg/undermine"),
                            "Takes you to the UnderMine discord server");
                    });

            SetLoading(true, "Fetching latest mods...", 100);
            HandleModRegistry();
        }

        private void SetLoading(bool loading, string text = "Loading...", int progress = 100)
        {
            if (loading)
            {
                lblLoading.Text = text;
                lblLoading.Visible = true;

                pbLoading.Value = progress;
                pbLoading.Visible = true;

                menuMain.Enabled = false;
                dgvMods.Enabled = false;
                return;
            }

            lblLoading.Text = text;
            lblLoading.Visible = false;

            pbLoading.Value = progress;
            pbLoading.Visible = false;

            menuMain.Enabled = true;
            dgvMods.Enabled = true;
        }

        private async void HandleModRegistry()
        {
            var mods = await _registry.GetRegistry();
            if (mods == null)
            {
                MessageBox.Show("Unable to get mod registry, are you connected to the internet?");
                return;
            }

            var loadedMods = _core.GetInstalledMods();
            if (loadedMods == null)
                loadedMods = new Dictionary<string, string[]>();

            _mods = new DataTable();
            _mods.Columns.Add("Guid", typeof(string));
            _mods.Columns.Add("Name", typeof(string));
            _mods.Columns.Add("Description", typeof(string));
            _mods.Columns.Add("Author", typeof(string));
            _mods.Columns.Add("Version", typeof(string));
            _mods.Columns.Add("Type", typeof(string));
            _mods.Columns.Add("Installed", typeof(bool));
            _mods.Columns.Add("mod", typeof(Mod));

            foreach(var mod in mods.Mods)
            {
                var installed = loadedMods.ContainsKey(mod.InstallSettings.Owner) &&
                                loadedMods[mod.InstallSettings.Owner].Contains(mod.InstallSettings.Repo);

                _mods.Rows.Add(mod.Guid,
                            mod.Name,
                            mod.Description,
                            mod.Author,
                            mod.GameVersion,
                            ModTypeString(mod.Type),
                            installed,
                            mod);
            }

            dgvMods.AutoGenerateColumns = false;
            dgvMods.Columns.Clear();
            dgvMods.Columns.AddRange
            (
                new DataGridViewCheckBoxColumn
                {
                    HeaderText = "Installed",
                    DataPropertyName = "Installed",
                    Name = "Installed",
                    ToolTipText = "Whether or not the mod is currently installed"
                },
                Column("Name", "The name of the mod"),
                Column("Description", "A description of what the mod does"),
                Column("Author", "The name of the person who made the mod"),
                Column("Version", "The version of the game this mod is intended for"),
                Column("Type", "A summary of what this mod is intended to do")
            );
            dgvMods.DataSource = _mods;

            SetLoading(false);
        }

        private DataGridViewTextBoxColumn Column(string name, string description)
        {
            return new DataGridViewTextBoxColumn
            {
                HeaderText = name,
                ToolTipText = description,
                DataPropertyName = name,
                Name = name,
                ReadOnly = true
            };
        }

        private string ModTypeString(ModType type)
        {
            var types = new List<string>();

            if (type.HasFlag(ModType.AdditionalEnemies))
                types.Add("Enemies");
            if (type.HasFlag(ModType.AdditionalItems))
                types.Add("Items");
            if (type.HasFlag(ModType.AdditionalRelics))
                types.Add("Relics");
            if (type.HasFlag(ModType.Cheats))
                types.Add("Cheats");
            if (type.HasFlag(ModType.GameBalance))
                types.Add("Balancing");

            return string.Join(", ", types);
        }

        private void MenuButton_VisitPage(string url)
        {
            _core.OpenWebPage(url);
        }

        private void MenuButton_ViewLogs()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var path = Path.Combine(currentDir, "Logs");
            _core.OpenWebPage(path);
        }

        private async void MenuButton_InstallUMC(bool latest = true)
        {
            if (!latest)
            {
                MessageBox.Show("This feature is coming soon!");
                return;
            }

            SetLoading(true, "Installing Under Mine Control...", 0);
            var worked = await _core.InstallUmc();
            SetLoading(false);

            if (!worked)
                MessageBox.Show("Something went wrong when installing UMC. Please check the logs!");
            else
                MessageBox.Show("UnderMineControl was successfully installed!");
        }

        private void MenuButton_UninstallUMC()
        {
            var res = MessageBox.Show("Are you sure you want to uninstall UnderMineControl AND all mods?",
                "Are you positive? :(",
                MessageBoxButtons.YesNo);

            if (res != DialogResult.Yes)
                return;

            SetLoading(true, "Uninstalling UnderMineControl....", 0);
            var worked = _core.Uninstall();

            HandleModRegistry();
            SetLoading(false);
            if (worked)
                MessageBox.Show("UnderMineControl has been uninstalled, sorry to see you go :(");
            else
                MessageBox.Show("Something went wrong while trying to uninstall UnderMineControl. Please check the logs!");

        }

        private void MenuButton_DisableMods()
        {
            var res = MessageBox.Show("Are you sure you want to delete all mods?",
                "Are you positive? :(",
                MessageBoxButtons.YesNo);

            if (res != DialogResult.Yes)
                return;

            SetLoading(true, "Uninstalling all mods...", 0);

            foreach(DataRow mod in _mods.Rows)
            {
                mod.SetField("Installed", false);
            }

            btnApplyChanges_Click(null, null);
        }

        private void MenuButton_PurgeFileCache()
        {
            _file.PurgeCache();
            MessageBox.Show("File cache purged!");
        }

        private void MenuButton_EnableConsole(ToolStripButton button)
        {
            var consoled = _core.IsConsoleEnabled();
            if (consoled == null)
            {
                MessageBox.Show("Please run the game at least once with UnderMineControl installed before attempting to activate the console!");
                return;
            }

            _core.ToggleConsole(!consoled.Value);
            button.Text = (_core.IsConsoleEnabled() ?? false) ? "Disable Console" : "Enable Console";
        }

        private void MenuButton_VerifyFiles()
        {
            var worked = _core.VerifyGameFiles();
            MessageBox.Show(worked ?
                "Steam should have started verifying UnderMine's game files! Check steam to see it's progress" :
                "Something seems to be stopping steam from verifying game files! Is steam running?");
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            var search = txtSearch.Text.Replace("'", "");
            _mods.DefaultView.RowFilter = $"Name LIKE '%{search}%' OR Description LIKE '%{search}%' OR Author LIKE '%{search}%'";
            dgvMods.Refresh();
        }

        private void btnApplyChanges_Click(object sender, EventArgs e)
        {
            SetLoading(true, "Syncing mods...", 0);

            new Thread(() =>
            {
                ModInstaller.ManageInstallChanges(_mods, 
                    (s, i) => this.InvokeAction(() => SetLoading(true, s, i)), 
                    _core,
                    _logger);

                this.InvokeAction(() => SetLoading(false));

                this.InvokeAction(() => HandleModRegistry());
            }).Start();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SetLoading(true, "Reseting mods list...", 100);
            HandleModRegistry();
        }
    }
}
