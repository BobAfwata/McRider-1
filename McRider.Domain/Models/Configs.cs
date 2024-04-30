namespace McRider.Domain.Models;

public class Configs
{
    public string? Id { get; set; } = "configs-" + Guid.NewGuid().ToString();
    public string PortName { get; set; } = "COM7";
    public int BaudRate { get; set; } = 9600;
    public int ReadTimeout { get; set; } = 5000;
}