using System.Diagnostics;

namespace Klipboard.Utils;

public static class ExceptionUtils
{
    public static void Protect(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    public static async Task Protect(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }


}
