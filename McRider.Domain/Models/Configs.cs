namespace McRider.Domain.Models;

public class Configs
{
    public string? Id { get; set; } = "configs-" + Guid.NewGuid().ToString();
    public DateTime ModifiedTime { get; set; } = DateTime.Parse("2024-05-12");
    public string PortName { get; set; } = "COM7";
    public int BaudRate { get; set; } = 9600;
    public int ReadTimeout { get; set; } = 500;
    public string Theme { get; set; } = "showmax";
}