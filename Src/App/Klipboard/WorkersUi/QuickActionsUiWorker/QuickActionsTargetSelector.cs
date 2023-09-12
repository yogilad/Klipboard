using Klipboard.Utils;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Klipboard.WorkersUi.QuickActionsUiWorker
{
    public partial class QuickActionsTargetSelector : Form
    {
        public bool UserConfirmedSelection;
        public int CurrentClusterIndex;
        public string CurrentDatabase;

        private List<Cluster> m_clusterList;

        public QuickActionsTargetSelector()
        {
            InitializeComponent();
        }

        public void Init(ISettings settings)
        {
            var config = settings.GetConfig();

            clusterComboBox.Items.Clear();
            m_clusterList = config.KustoConnectionStrings;
            m_clusterList.ForEach(c => clusterComboBox.Items.Add(c.ConnectionString));

            clusterComboBox.SelectedIndex = config.DefaultClusterIndex;
            databaseTextBox.Text = config.ChosenCluster.DatabaseName;
        }

        private void clusterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentClusterIndex = clusterComboBox.SelectedIndex;
            databaseTextBox.Text = m_clusterList[CurrentClusterIndex].DatabaseName;
        }

        private void databaseNameTextBox_TextChanged(object sender, EventArgs e)
        {
            CurrentDatabase = databaseTextBox.Text;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            UserConfirmedSelection = true;
            Close();
        }
    }
}
