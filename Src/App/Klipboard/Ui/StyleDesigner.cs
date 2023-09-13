namespace Klipboard
{
    internal static class StyleDesigner
    {
        public static void SetFormDesign(Form form)
        {
            form.Icon = ResourceLoader.GetIcon();
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.ShowInTaskbar = false;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
        }
    }
}
