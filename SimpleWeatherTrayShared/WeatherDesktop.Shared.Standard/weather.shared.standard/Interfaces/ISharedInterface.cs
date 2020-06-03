
namespace WeatherDesktop.Interface
{
    public interface ISharedInterface
    {
        ISharedResponse Invoke();
        string Debug();
        System.Collections.Generic.Dictionary<string, System.EventHandler> SettingsItems();
        System.Exception ThrownException();
        void Load();
    }
}
