namespace Klipboard
{
    public partial class InspectForm: Form
    {
        public InspectForm(string content)
        {
            InitializeComponent();

            textBox.Text = content;
        }
    }
}
