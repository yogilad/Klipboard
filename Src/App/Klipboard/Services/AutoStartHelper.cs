using Microsoft.Win32;


namespace Klipboard
{
    public class AutoStartHelper
    {
        public static bool TryGetAutoStartEnabled(out bool enabled)
        {
            try
            {
                var rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                var startApp = rk?.GetValue(nameof(Klipboard), "") as string;
                
                enabled = string.Equals(startApp, Application.ExecutablePath);
                return true;
            } 
            catch 
            {
                enabled = false;
                return false;
            }
        }

        public static bool TrySetAutoStart(bool enableAutoStart)
        {
            try
            {
                if (TryGetAutoStartEnabled(out var isEnabled) && isEnabled != enableAutoStart)
                {
                    var rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                    if (rk == null)
                    {
                        return false;
                    }

                    if (enableAutoStart)
                    {
                        rk.SetValue(nameof(Klipboard), Application.ExecutablePath);
                    }
                    else
                    {
                        rk.DeleteValue(nameof(Klipboard), false);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
