namespace McRider.Domain.Services;

public interface IForegroundService
{
    void Start(string title = null, string message = null);
    void Stop();
}
