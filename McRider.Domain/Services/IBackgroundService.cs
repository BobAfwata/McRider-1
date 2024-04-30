namespace McRider.Domain.Services;

public interface IBackgroundService
{
    void Start(string title = null, string message = null);
    void Stop();
}
