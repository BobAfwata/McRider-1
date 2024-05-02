namespace McRider.Domain.Models;

public class Player
{
    public string? Id { get; set; } = "player-" + Guid.NewGuid().ToString();

    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Nickname { get; set; }
    public string Team { get; set; }
    public string Gender { get; set; }

    [JsonIgnore]
    public bool IsActive { get; set; } = true;

    public override string ToString() => Nickname;

}
