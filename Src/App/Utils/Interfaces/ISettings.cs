namespace Klipboard.Utils
{
    public interface ISettings
    {
        AppConfig GetConfig();
        Task UpdateConfig(AppConfig config);
    }
}
