namespace McRider.MAUI.Services;

public interface IBackgroundService
{
    void Start(string title = null, string message = null);
    void Stop();
}

public interface IForegroundService
{
    void Start(string title = null, string message = null);
    void Stop();
}
