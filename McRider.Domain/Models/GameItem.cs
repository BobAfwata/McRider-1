using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.Domain.Models;

public class GameItem
{
    public string? Id { get; set; } = "gameItem-" + Guid.NewGuid().ToString();
    public string Image { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool TeamUp { get; set; }
    public int TeamsCount { get; set; }
    public int PlayersPerTeam { get; set; }
    public double? TargetDistance { get; set; }
    public TimeSpan? TargetTime { get; set; }
    public bool AllowLosserToFinish { get; set; } = false;
}
