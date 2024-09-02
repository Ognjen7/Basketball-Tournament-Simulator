using Basketball_Tournament_Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basketball_Tournament_Simulation;

public class KnockoutMatchSimulation
{
    public static BasketballTeam SimulateKnockoutMatch(BasketballTeam team1, BasketballTeam team2, Dictionary<string, List<MatchResult>> preseasonResults)
    {
        Random random = new Random();

        int rankDifference = Math.Abs(team1.FIBARanking - team2.FIBARanking);

        // Racunanje koeficijenata forme za obe ekipe

        int formCoefficientTeam1 = FormCoefficient.CalculateFormCoefficient(team1.ISOCode, preseasonResults, team1.TournamentWins);
        int formCoefficientTeam2 = FormCoefficient.CalculateFormCoefficient(team2.ISOCode, preseasonResults, team2.TournamentWins);

        double winProbabilityTeam1 = 0.5 + ((formCoefficientTeam1 - formCoefficientTeam2) * 0.05) + 0.01 * (team2.FIBARanking - team1.FIBARanking) * 1.75;

        if (team1.ISOCode == "USA")
        {
            winProbabilityTeam1 += 0.2;
        }

        bool team1Wins = random.NextDouble() < winProbabilityTeam1;

        int basePoints = random.Next(70, 110);

        int team1Points;
        int team2Points;

        if (team1Wins)
        {
            team1Points = basePoints + random.Next(1, 15);
            team2Points = basePoints - random.Next(0, 2);
            team1.TournamentWins++;

            team1Points += rankDifference / 2;
        }
        else
        {
            team2Points = basePoints + random.Next(1, 15);
            team1Points = basePoints - random.Next(0, 2);
            team2Points += rankDifference / 2;

            team2.TournamentWins++;
        }

        Console.WriteLine($"{team1.Team} - {team2.Team} ({team1Points} : {team2Points})");

        return team1Wins ? team1 : team2;
    }
}
