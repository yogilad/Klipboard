namespace Klipboard
{
    public partial class InspectForm : Form
    {
        public InspectForm(string content)
        {
            InitializeComponent();
            StyleDesigner.SetFormDesign(this);
            textBox.Text = content;
        }
    }
}
