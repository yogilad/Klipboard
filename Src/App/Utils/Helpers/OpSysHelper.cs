﻿using System.Diagnostics;


namespace Klipboard.Utils
{
    public class AutoDispose : IDisposable
    {
        Action? m_disposeAction;

        public AutoDispose()
        {
            m_disposeAction = null;
        }

        public AutoDispose(Action disposeAction)
        {
            m_disposeAction = disposeAction;
        }

        public void Dispose() 
        {
            m_disposeAction?.Invoke();
            m_disposeAction = null;
        }
    }

    public static class OpSysHelper
    {
        public static void InvokeLink(string link)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            });

            return;
        }

        public static bool TryAcquireSingleProcessLock(out AutoDispose? processLock)
        {
#if DEBUG
            var name = $"{AppConstants.ApplicationName}_DEBUG";
#else
            var name = $"{AppConstants.ApplicationName}_RELEASE";
#endif

            if (Debugger.IsAttached)
            {
                processLock = new AutoDispose();
                return true;
            }

            var singleProcessMutex = new Mutex(initiallyOwned: false, name);

            if (!singleProcessMutex.WaitOne(0))
            {
                singleProcessMutex.Dispose();
                processLock = null;
                return false;
            }

            processLock = new AutoDispose(() => singleProcessMutex.ReleaseMutex());
            return true;
        }

        public static string AppFolderPath()
        {
            var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppConstants.ApplicationName);
            return appDir;
        }
    }
}
