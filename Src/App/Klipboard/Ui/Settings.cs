using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Klipboard.Utils;

namespace Klipboard
{
    public partial class Settings : Form, ISettings
    {
        // These are guaranteed to be initialized in the Init method, and the constructor is private
        private AppConfigFile m_appConfigFile = null!;
        private AppConfig m_config = null!;

        private Settings()
        {
            InitializeComponent();
        }

        public static async Task<Settings> Init()
        {
            var settings = new Settings();
            await settings.LoadSettingsToUi().ConfigureAwait(false);
            return settings;
        }

        private async Task LoadSettingsToUi()
        {
            UpdateConfigPath();
            await ReadConfigFromPath().ConfigureAwait(false);
            DataToUi();
        }

        private async Task ReadConfigFromPath()
        {
            m_config = await m_appConfigFile.Read().ConfigureAwait(false);
        }

        private void UpdateConfigPath()
        {
            var path = m_appConfigFile?.ConfigPath ?? Properties.Settings.Default.SettingsPath;
            m_appConfigFile = string.IsNullOrWhiteSpace(path) ? new AppConfigFile() : new AppConfigFile(path);
            Properties.Settings.Default.SettingsPath = m_appConfigFile.ConfigPath;
            txtSettingsPath.Text = Properties.Settings.Default.SettingsPath;
        }

        private ConfiguredTaskAwaitable<bool> SaveSettings()
        {
            m_config = UiToData();
            return m_appConfigFile.Write(m_config).ConfigureAwait(false);
        }

        private void DataToUi()
        {
            lstClusters.Items.Clear();
            foreach (var cluster in m_config.KustoConnectionStrings)
            {
                lstClusters.Items.Add(
                    new ListViewItem(
                        new[] { cluster.ConnectionString, cluster.DatabaseName }
                    ));
            }
            lstClusters.SelectedItems.Clear();
            if (m_config.DefaultClusterIndex >= 0 && m_config.DefaultClusterIndex < lstClusters.Items.Count)
            {
                lstClusters.Items[m_config.DefaultClusterIndex].Selected = true;
            }

            chkStartWithWindows.Checked = m_config.StartAutomatically;
            cmbApp.Items.Clear();
            string[] strings = Enum.GetNames(typeof(QueryApp));
            cmbApp.Items.AddRange(strings);
            cmbApp.SelectedIndex = (int)m_config.DefaultQueryApp;
            txtQuery.Text = m_config.PrependFreeTextQueriesWithKql;
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

        #region Events
        private async void btnLoadSettings_Click(object sender, EventArgs e)
        {
            await ExceptionUtils.Protect(async () => await LoadSettingsToUi()).ConfigureAwait(false);
        }

        private void lstClusters_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExceptionUtils.Protect(() =>
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
            });
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            ExceptionUtils.Protect(() =>
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
            });
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ExceptionUtils.Protect(() =>
            {
                var item = new ListViewItem(new[] { txtConnectionStr.Text, txtDatabase.Text });
                lstClusters.Items.Add(item);
            });
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            await ExceptionUtils.Protect(async () => await SaveSettings());
        }
        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            await ExceptionUtils.Protect(async () =>
            {
                // Show a message box asking users if they want to save
                if (MessageBox.Show(@"Do you want to save your changes?", @"Save changes", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    await SaveSettings();
                }
            }).ConfigureAwait(false);

        }
        #endregion


        #region ISettings

        public AppConfig GetConfig() => m_config;

        public async Task UpdateConfig(AppConfig config)
        {
            m_config = config;
            DataToUi();
            await SaveSettings();
        }

        #endregion


    }
}
