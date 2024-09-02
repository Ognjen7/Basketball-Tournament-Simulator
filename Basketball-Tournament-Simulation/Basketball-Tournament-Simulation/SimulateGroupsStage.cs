using Basketball_Tournament_Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basketball_Tournament_Simulation;

public class SimulateGroupsStage
{
    public static void SimulateGroupMatches(string groupName, List<BasketballTeam> teams, Dictionary<string, List<MatchResult>> preseasonResults)
    {
        Console.WriteLine($"\nGrupa {groupName}:\n");

        // Simulacija meceva za sve kombinacije timova u okviru grupe
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                BasketballTeam team1 = teams[i];
                BasketballTeam team2 = teams[j];
                MatchSimulation.SimulateMatch(team1, team2, preseasonResults);
            }
        }

        Console.WriteLine();
    }

    public static void PrintGroupResults(string groupName, List<BasketballTeam> teams)
    {
        Console.WriteLine($"Konacni poredak u Grupi {groupName}:\n");

        Console.WriteLine($"Ime - pobede/porazi/bodovi/postignuti koševi/primljeni koševi/koš razlika");

        int rank = 1;

        int maxTeamNameLength = teams.Max(t => t.Team.Length);

        foreach (var team in teams)
        {
            string formattedTeamName = team.Team.PadRight(maxTeamNameLength);

            Console.WriteLine($"{rank,2}. {formattedTeamName} {team.Wins} / {team.Losses} / {team.Points} / {team.ScoredPoints} / {team.ConcededPoints} / {team.PointDifference:+#;-#;0}");

            rank++;
        }
    }
}
