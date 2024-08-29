using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basketball_Tournament_Simulation.Models;

public class Match
{
    public BasketballTeam Team1 { get; set; }
    public BasketballTeam Team2 { get; set; }
    public int Team1Score { get; set; }
    public int Team2Score { get; set; }

    public Match(BasketballTeam team1, BasketballTeam team2)
    {
        Team1 = team1;
        Team2 = team2;
    }
}
