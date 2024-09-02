using Basketball_Tournament_Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basketball_Tournament_Simulation;

public class MatchSimulation
{
    public static void SimulateMatch(BasketballTeam team1, BasketballTeam team2, Dictionary<string, List<MatchResult>> preseasonResults)
    {
        Random random = new Random();

        int rankDifference = Math.Abs(team1.FIBARanking - team2.FIBARanking);

        // Racunanje koeficijenta forme za timove na osnovu rezultata iz predsezone
        int formCoefficientTeam1 = FormCoefficient.CalculateFormCoefficient(team1.ISOCode, preseasonResults, team1.TournamentWins);
        int formCoefficientTeam2 = FormCoefficient.CalculateFormCoefficient(team2.ISOCode, preseasonResults, team2.TournamentWins);

        // Racunanje osnovne verovatnoce za pobedu timova
        double baseWinProbabilityTeam1 = 0.5 + (formCoefficientTeam1 * 0.05) - (formCoefficientTeam2 * 0.05);
        double baseWinProbabilityTeam2 = 0.5 + (formCoefficientTeam2 * 0.05) - (formCoefficientTeam1 * 0.05);

        // Povecavanje osnovne verovatnoce da USA pobedi protiv svakog protivnika usled neravnomernog odnosa snaga i realnijih rezultata
        if (team1.ISOCode == "USA")
        {
            baseWinProbabilityTeam1 *= 1.2;
        }
        else if (team2.ISOCode == "USA")
        {
            baseWinProbabilityTeam2 *= 1.2;
        }

        // Sansa da se neki tim preda
        double forfeitChance = 0.01;
        bool team1Forfeits = random.NextDouble() < forfeitChance;
        bool team2Forfeits = random.NextDouble() < forfeitChance;

        if (team1Forfeits && team2Forfeits)
        {
            team1Forfeits = random.NextDouble() < 0.5;
            team2Forfeits = !team1Forfeits;
        }

        if (team1Forfeits)
        {
            team1.Forfeits++;
            team1.Losses++;
            team2.Wins++;
            team1.Points += 0;
            team2.Points += 2;


            int team1Points = 0;
            int team2Points = 20;

            team1.ScoredPoints += team1Points;
            team1.ConcededPoints += team2Points;
            team2.ScoredPoints += team2Points;
            team2.ConcededPoints += team1Points;

            Console.WriteLine($"{team1.Team} forfeits the match against {team2.Team}. Final score: {team1Points} : {team2Points}");
            return;
        }
        else if (team2Forfeits)
        {

            team2.Forfeits++;
            team2.Losses++;
            team1.Wins++;
            team1.Points += 2;
            team2.Points += 0;


            int team1Points = 20;
            int team2Points = 0;

            team1.ScoredPoints += team1Points;
            team1.ConcededPoints += team2Points;
            team2.ScoredPoints += team2Points;
            team2.ConcededPoints += team1Points;

            Console.WriteLine($"{team2.Team} forfeits the match against {team1.Team}. Final score: {team1Points} : {team2Points}");
            return;
        }

        // Nastavak normanog toka simulacije ako se nijedan tim ne preda
        bool team1Wins = random.NextDouble() < baseWinProbabilityTeam1;

        int basePoints = random.Next(70, 110);

        int team1Score;
        int team2Score;

        if (team1Wins)
        {
            team1Score = basePoints + random.Next(1, 15);
            team2Score = basePoints - random.Next(0, 2);

            team1.Points += 2;
            team1.Wins++;
            team2.Points += 1;
            team2.Losses++;

            team1Score += rankDifference / 2;
        }
        else
        {
            team2Score = basePoints + random.Next(1, 15);
            team1Score = basePoints - random.Next(0, 2);

            team2.Points += 2;
            team2.Wins++;
            team1.Points += 1;
            team1.Losses++;

            team2Score += rankDifference / 2;
        }

        team1.ScoredPoints += team1Score;
        team1.ConcededPoints += team2Score;
        team2.ScoredPoints += team2Score;
        team2.ConcededPoints += team1Score;

        Console.WriteLine($"{team1.Team} - {team2.Team} ({team1Score} : {team2Score})");
    }
}
