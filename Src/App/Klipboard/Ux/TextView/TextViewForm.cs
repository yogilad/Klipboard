namespace Klipboard
{
    public partial class TextViewForm : Form
    {
        private static string TruncationMessage = $"...{Environment.NewLine}{Environment.NewLine}( Remaining Content Truncated )";

        public TextViewForm(string title, string status, string content, bool wordWrap = false)
        {
            InitializeComponent();
            StyleDesigner.SetDialogDesign(this);

            if (content.Length > textBox.MaxLength - TruncationMessage.Length)
            {
                content = content.Substring(0, textBox.MaxLength - TruncationMessage.Length) + TruncationMessage;
            }

            Text = title;
            toolStripStatusLabel.Text = status;
            textBox.Text = content;
            textBox.WordWrap = wordWrap;
        }

        private void InspectForm_Load(object sender, EventArgs e)
        {
            textBox.Select(0, 0);
        }
    }
}
