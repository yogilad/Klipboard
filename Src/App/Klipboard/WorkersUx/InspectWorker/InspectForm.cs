namespace Klipboard
{
    public partial class InspectForm : Form
    {
        private static string TruncationMessage = $"...{Environment.NewLine}{Environment.NewLine}( Remaining Content Truncated )";

        public InspectForm(string contentType, string size, string content)
        {
            InitializeComponent();
            StyleDesigner.SetDialogDesign(this);

            if (content.Length > textBox.MaxLength - TruncationMessage.Length)
            {
                content = content.Substring(0, textBox.MaxLength - TruncationMessage.Length) + TruncationMessage;
            }

            this.toolStripLabel1.Text = contentType;
            this.toolStripLabel2.Text = size;
            textBox.Text = content;
        }

        private void InspectForm_Load(object sender, EventArgs e)
        {
            textBox.Select(0, 0);
        }
    }
}
