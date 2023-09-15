using System.Diagnostics.CodeAnalysis;

namespace Klipboard
{
    internal static class StyleDesigner
    {
        public static void SetDialogDesign(Form form)
        {
            SetBaseDesigner(form, appearInTaskbar: true, allowMinimize: false, allowClose: true);
        }

        public static void SetProgressDesign(Form form)
        {
            SetBaseDesigner(form, appearInTaskbar: true, allowMinimize: true, allowClose: false);
        }

        public static void SetMessageDesign(Form form)
        {
            SetBaseDesigner(form, appearInTaskbar: false, allowMinimize: false, allowClose: true);
            
        }

        private static void SetBaseDesigner(Form form, bool appearInTaskbar, bool allowMinimize, bool allowClose)
        {
            form.Icon = ResourceLoader.KustoColorIcon;
            form.MaximizeBox = false;
            form.MinimizeBox = allowMinimize;
            form.ShowInTaskbar = appearInTaskbar;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
        }
    }
}
