namespace Klipboard
{
    public partial class ClipboardContentForm : Form
    {
        public ClipboardContentForm(string content)
        {
            InitializeComponent();

            textBox.Text = content;
        }
    }
}