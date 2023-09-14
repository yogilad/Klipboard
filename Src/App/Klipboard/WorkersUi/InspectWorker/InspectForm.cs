namespace Klipboard
{
    public partial class InspectForm : Form
    {
        public InspectForm(string content)
        {
            InitializeComponent();
            StyleDesigner.SetDialogDesign(this);
            textBox.Text = content;
        }
    }
}
