using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Klipboard.Workers;

namespace Klipboard
{
    public partial class IngestForm : Form
    {
        public IngestWorkerBase.IngestUserSelection UserSelection = new IngestWorkerBase.IngestUserSelection();

        public IngestForm(string title, bool devMode)
        {
            InitializeComponent();
            StyleDesigner.SetDialogDesign(this);

            // Set the form title
            this.Text = title;

            // Set dev mode UX components 
            ParallelismLabel.Visible = devMode;
            ParallelismTrackBar.Visible = devMode;

            // Set Table selector mode 
            ExitingTableRadio.Checked = true;
            TableTextBox.Visible = false;
            TableSchemaTextBox.Enabled = false;

            // Set mapping mode
            NoMappingRadio.Checked = true;
            MappingTextBox.Enabled = false;
        }
    }
}
