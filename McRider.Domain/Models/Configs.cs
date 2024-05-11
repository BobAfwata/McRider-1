namespace McRider.Domain.Models;

public class Configs
{
    public string? Id { get; set; } = "configs-" + Guid.NewGuid().ToString();
    public DateTime ModifiedTime { get; set; } = DateTime.Parse("2000-01-01");
    public string PortName { get; set; } = "COM7";
    public int BaudRate { get; set; } = 9600;
    public int ReadTimeout { get; set; } = 5000;
}