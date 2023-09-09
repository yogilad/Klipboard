using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Klipboard.Utils;

namespace Klipboard
{
    public partial class Settings : Form, ISettings
    {
        private AppConfigFile? _appConfigFile;
        private AppConfig _config;

        public Settings()
        {
            InitializeComponent();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadSettings();
            txtSettingsPath.Text = Properties.Settings.Default.SettingsPath;
        }

        public async Task LoadSettings()
        {
            var config = await InitSettings().ConfigureAwait(false);
            DataToUi(config);
        }

        public async Task<AppConfig> InitSettings()
        {
            var path = _appConfigFile?.ConfigPath ?? Properties.Settings.Default.SettingsPath;
            _appConfigFile = string.IsNullOrWhiteSpace(path) ? new AppConfigFile() : new AppConfigFile(path);
            Properties.Settings.Default.SettingsPath = _appConfigFile?.ConfigPath ?? Properties.Settings.Default.SettingsPath;
            _config = await _appConfigFile.Read().ConfigureAwait(false);
            SaveSettings(_config);
            return _config;
        }


        private async void btnLoadSettings_Click(object sender, EventArgs e)
        {
            try { await LoadSettings(); }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private ConfiguredTaskAwaitable<bool> SaveSettings(AppConfig? config = null)
        {
            var configToSave = config ?? UiToData();
            return _appConfigFile.Write(configToSave).ConfigureAwait(false);
        }

        private void DataToUi(AppConfig config)
        {
            lstClusters.Items.Clear();
            foreach (var cluster in config.KustoConnectionStrings)
            {
                lstClusters.Items.Add(
                    new ListViewItem(
                        new[] { cluster.ConnectionString, cluster.DatabaseName }
                    ));
            }
            lstClusters.SelectedItems.Clear();
            if (config.DefaultClusterIndex >= 0 && config.DefaultClusterIndex < lstClusters.Items.Count)
            {
                lstClusters.Items[config.DefaultClusterIndex].Selected = true;
            }

            chkStartWithWindows.Checked = config.StartAutomatically;
            cmbApp.Items.Clear();
            cmbApp.Items.AddRange(Enum.GetNames(typeof(QueryApp)));
            cmbApp.SelectedIndex = (int)config.DefaultQueryApp;
            txtQuery.Text = config.PrependFreeTextQueriesWithKql;
        }

        private AppConfig UiToData()
        {
            var clusters = lstClusters.Items.Cast<ListViewItem>().Select(item => new Cluster(item.SubItems[clmConnectionString.Index].Text, item.SubItems[clmDb.Index].Text)).ToList();
            var defaultClusterIndex = lstClusters.SelectedItems.Count > 0 ? lstClusters.SelectedItems[0].Index : 0;
            var startAutomatically = chkStartWithWindows.Checked;
            var defaultQueryApp = (QueryApp)cmbApp.SelectedIndex;
            var prependFreeTextQueriesWithKql = txtQuery.Text;

            return new AppConfig(clusters, defaultClusterIndex, defaultQueryApp, startAutomatically, prependFreeTextQueriesWithKql);
        }

        private void lstClusters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstClusters.SelectedItems.Count == 0)
            {
                return;
            }
            var selected = lstClusters.SelectedItems[0];
            if (selected == null)
            {
                return;
            }
            txtConnectionStr.Text = selected.SubItems[0].Text;
            txtDatabase.Text = selected.SubItems[1].Text;
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            var selectedItems = lstClusters.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return;
            }

            var selected = lstClusters.SelectedItems[0];
            if (selected == null)
            {
                return;
            }
            selected.SubItems[0].Text = txtConnectionStr.Text;
            selected.SubItems[1].Text = txtDatabase.Text;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var item = new ListViewItem(new[] { txtConnectionStr.Text, txtDatabase.Text });
            lstClusters.Items.Add(item);
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            await SaveSettings();
        }

        public AppConfig GetConfig() => _config;

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            // Show a message box asking users if they want to save
            if (MessageBox.Show("Do you want to save your changes?", "Save changes", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await SaveSettings();
            }
        }
    }
}
