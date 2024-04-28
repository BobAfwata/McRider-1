namespace McRider.MAUI.Messages;

public class ProgressMessage
{
    public string Message { get; set; }
    public double Progress { get; set; }
    public string Title { get; internal set; }
}
