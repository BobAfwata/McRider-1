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
    public GameType GameType { get; set; }
    public int TeamsCount { get; set; }
    public int PlayersPerTeam { get; set; }
    public double? TargetDistance { get; set; }
    public TimeSpan? TargetTime { get; set; }
    public bool AllowLosserToFinish { get; set; } = false;
    public bool IsActive { get; set; } = false;
    public List<string> RevealImages { get; set; } = [];
    public bool? HorizontalProgress { get; set; }
    public int CountDown { get; set; } = 3;
}

public enum GameType
{
    Tournament,
    Team,
    RevealChallenge,
    RacingChallenge,
    SampleChallenge,
    DistanceChallenge,
}
