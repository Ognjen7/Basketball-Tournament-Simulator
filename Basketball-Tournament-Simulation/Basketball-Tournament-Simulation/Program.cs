using Basketball_Tournament_Simulation.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class Program
{
    static void Main(string[] args)
    {
        // Ucitavanje JSON podataka za grupe
        var jsonFilePath = "groups.json";
        var jsonString = File.ReadAllText(jsonFilePath);

        var groups = JsonSerializer.Deserialize<Dictionary<string, List<BasketballTeam>>>(jsonString);

        // Ucitavanje JSON podataka za predsezonske utakmice
        var preseasonResultsFilePath = "exibitions.json";
        var preseasonResultsString = File.ReadAllText(preseasonResultsFilePath);

        var preseasonResults = JsonSerializer.Deserialize<Dictionary<string, List<MatchResult>>>(preseasonResultsString);

        if (groups != null)
        {
            Console.WriteLine("Mečevi grupne faze:\n");

            Dictionary<string, string> teamGroupMembership = new Dictionary<string, string>();

            // Simulacija meceva grupne faze
            foreach (var group in groups)
            {
                string groupName = group.Key;
                List<BasketballTeam> teams = group.Value;

                foreach (var team in teams)
                {
                    teamGroupMembership[team.Team] = groupName;
                }

                SimulateGroupMatches(groupName, teams, preseasonResults);
                RankTeams(teams);
                PrintGroupResults(groupName, teams);
            }

            // Odredjivanje timova koji prolaze u eliminacionu fazu

            List<BasketballTeam> advancingTeams = RankAdvancingTeams(groups);

            Console.WriteLine("\nTimovi koji prolaze u eliminacionu fazu:");
            foreach (var team in advancingTeams)
            {
                Console.WriteLine($"{team.Team} ({team.ISOCode})");
            }

            Console.WriteLine("\n--------------------------------------------------------------------\n");

            // Perform the draw and simulate knockout stage
            Console.WriteLine("Eliminaciona faza - parovi i mecevi:\n");
            PerformKnockoutStage(advancingTeams, teamGroupMembership, preseasonResults);
        }
        else
        {
            Console.WriteLine("Error: Unable to read or parse groups.json file.");
        }
    }

    // Metoda koja simulira sve meceve u grupnoj fazi
    static void SimulateGroupMatches(string groupName, List<BasketballTeam> teams, Dictionary<string, List<MatchResult>> preseasonResults)
    {
        Console.WriteLine($"\nGrupa {groupName}:\n");

        // Simulacija meceva za sve kombinacije timova u okviru grupe
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                BasketballTeam team1 = teams[i];
                BasketballTeam team2 = teams[j];
                SimulateMatch(team1, team2, preseasonResults);
            }
        }

        Console.WriteLine();
    }

    // Metoda koja simulira mec izmedju dva tima
    static void SimulateMatch(BasketballTeam team1, BasketballTeam team2, Dictionary<string, List<MatchResult>> preseasonResults)
    {
        Random random = new Random();

        int rankDifference = Math.Abs(team1.FIBARanking - team2.FIBARanking);

        // Racunanje koeficijenta forme za timove, na osnovu rezultata sa pripremnih utakmica
        int formCoefficientTeam1 = CalculateFormCoefficient(team1.ISOCode, preseasonResults);
        int formCoefficientTeam2 = CalculateFormCoefficient(team2.ISOCode, preseasonResults);

        // Racunanje osnovne verovatnoce pobede za oba tima
        double baseWinProbabilityTeam1 = 0.5 + (formCoefficientTeam1 * 0.05) - (formCoefficientTeam2 * 0.05);
        double baseWinProbabilityTeam2 = 0.5 + (formCoefficientTeam2 * 0.05) - (formCoefficientTeam1 * 0.05);

        //Povecavanje verovatnoce pobede za USA 

        if (team1.ISOCode == "USA")
        {
            baseWinProbabilityTeam1 *= 1.2;
        }
        else if (team2.ISOCode == "USA")
        {
            baseWinProbabilityTeam2 *= 1.2;
        }

        // Promena verovatnoce pobede na osnovu FIBA rankinga
        baseWinProbabilityTeam1 += 0.01 * rankDifference * (team1.FIBARanking < team2.FIBARanking ? 1.75 : -1.75);
        baseWinProbabilityTeam2 += 0.01 * rankDifference * (team2.FIBARanking < team1.FIBARanking ? 1.75 : -1.75);
        

        double totalProbability = baseWinProbabilityTeam1 + baseWinProbabilityTeam2;
        double winProbabilityTeam1 = baseWinProbabilityTeam1 / totalProbability;
        double winProbabilityTeam2 = baseWinProbabilityTeam2 / totalProbability;

        // Odredjivanje ishoda meca
        bool team1Wins = random.NextDouble() < winProbabilityTeam1;

        int basePoints = random.Next(70, 110);

        int team1Points;
        int team2Points;

        if (team1Wins)
        {
            team1Points = basePoints + random.Next(1, 15);
            team2Points = basePoints - random.Next(0, 5);

            team1.Points += 2;
            team1.Wins++;
            team2.Points += 1;
            team2.Losses++;

            team1Points += rankDifference / 2;
        }
        else
        {
            team2Points = basePoints + random.Next(1, 15);
            team1Points = basePoints - random.Next(1, 5);

            team2.Points += 2;
            team2.Wins++;
            team1.Points += 1;
            team1.Losses++;

            team2Points += rankDifference / 2;
        }

        team1.ScoredPoints += team1Points;
        team1.ConcededPoints += team2Points;
        team2.ScoredPoints += team2Points;
        team2.ConcededPoints += team1Points;

        Console.WriteLine("-------------------------------------------------------");
        Console.WriteLine($"winProbabilityTeam1 - {winProbabilityTeam1}");
        Console.WriteLine($"winProbabilityTeam2 - {winProbabilityTeam2}");
        Console.WriteLine($"formCoefficientTeam1 - {formCoefficientTeam1}");
        Console.WriteLine($"formCoefficientTeam2 - {formCoefficientTeam2}");
        Console.WriteLine($"{team1.Team} - {team2.Team} ({team1Points} : {team2Points})");
    }

    static int CalculateFormCoefficient(string isoCode, Dictionary<string, List<MatchResult>> preseasonResults)
    {
        if (preseasonResults.TryGetValue(isoCode, out var matches))
        {
            int wins = matches.Count(match =>
            {
                var scores = match.Result.Split('-').Select(int.Parse).ToArray();
                return scores[0] > scores[1];
            });

            return wins;
        }

        return 0;
    }

    static void PrintGroupResults(string groupName, List<BasketballTeam> teams)
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

    static List<BasketballTeam> RankAdvancingTeams(Dictionary<string, List<BasketballTeam>> groups)
    {
        // Liste koje sadrze prvo, drugo i treceplasiranog iz svake grupe
        List<BasketballTeam> firstPlaceTeams = new List<BasketballTeam>();
        List<BasketballTeam> secondPlaceTeams = new List<BasketballTeam>();
        List<BasketballTeam> thirdPlaceTeams = new List<BasketballTeam>();

        // Separacija timova prema pozicijama na kojima su zavrsili grupnu fazu
        foreach (var group in groups.Values)
        {
            firstPlaceTeams.Add(group[0]);
            secondPlaceTeams.Add(group[1]);
            thirdPlaceTeams.Add(group[2]);
        }

        // Rangiranje timova unutar setova po poenima, kos razlici itd
        RankTeams(firstPlaceTeams);
        RankTeams(secondPlaceTeams);
        RankTeams(thirdPlaceTeams);

        // Spajanje svih timova u jednu listu
        List<BasketballTeam> allRankedTeams = new List<BasketballTeam>();
        allRankedTeams.AddRange(firstPlaceTeams);  // Top 3 tima se rangiraju 1-3
        allRankedTeams.AddRange(secondPlaceTeams); // Sledeca 3 se rangiraju 4-6
        allRankedTeams.AddRange(thirdPlaceTeams);  // Poslednja 3 se rangiraju 7-9

        // Brisanje 9. tima
        allRankedTeams.RemoveAt(8);

        return allRankedTeams;
    }

    static void RankTeams(List<BasketballTeam> teams)
    {
        teams.Sort((x, y) =>
        {
            int pointComparison = y.Points.CompareTo(x.Points);
            if (pointComparison != 0) return pointComparison;

            int pointDiffComparison = y.PointDifference.CompareTo(x.PointDifference);
            if (pointDiffComparison != 0) return pointDiffComparison;

            return x.FIBARanking.CompareTo(y.FIBARanking);
        });
    }

    // Metoda koja simulira eliminacionu fazu i meceve u okviru nje
    static void PerformKnockoutStage(List<BasketballTeam> advancingTeams, Dictionary<string, string> teamGroupMembership, Dictionary<string, List<MatchResult>> preseasonResults)
    {
        Random random = new Random();

        // Kreiranje sesira po rankingu
        List<BasketballTeam> potD = advancingTeams.Take(2).ToList();
        List<BasketballTeam> potE = advancingTeams.Skip(2).Take(2).ToList();
        List<BasketballTeam> potF = advancingTeams.Skip(4).Take(2).ToList();
        List<BasketballTeam> potG = advancingTeams.Skip(6).Take(2).ToList();

        Console.WriteLine("Šeširi:");
        Console.WriteLine("\nŠešir D:");
        foreach (var team in potD)
        {
            Console.WriteLine($"{team.Team} ({team.ISOCode})");
        }

        Console.WriteLine("\nŠešir E:");
        foreach (var team in potE)
        {
            Console.WriteLine($"{team.Team} ({team.ISOCode})");
        }

        Console.WriteLine("\nŠešir F:");
        foreach (var team in potF)
        {
            Console.WriteLine($"{team.Team} ({team.ISOCode})");
        }

        Console.WriteLine("\nŠešir G:");
        foreach (var team in potG)
        {
            Console.WriteLine($"{team.Team} ({team.ISOCode})");
        }

        var quarterFinals = new List<Tuple<BasketballTeam, BasketballTeam>>();

        var remainingTeams = new List<BasketballTeam>();

        remainingTeams.AddRange(potD);
        remainingTeams.AddRange(potE);
        remainingTeams.AddRange(potF);
        remainingTeams.AddRange(potG);

        Console.WriteLine("\n--------------------------------------------------------------------\n");

        Console.WriteLine("Četvrtfinale parovi:\n");

        while (remainingTeams.Count > 1)
        {
            var team1 = remainingTeams[0];
            BasketballTeam team2 = null;

            for (int i = 1; i < remainingTeams.Count; i++)
            {
                var potentialTeam2 = remainingTeams[i];
                if (!HavePlayedInGroupStage(team1, potentialTeam2, teamGroupMembership))
                {
                    team2 = potentialTeam2;
                    break;
                }
            }

            if (team2 != null)
            {
                quarterFinals.Add(Tuple.Create(team1, team2));
                remainingTeams.Remove(team1);
                remainingTeams.Remove(team2);
                Console.WriteLine($"{team1.Team} vs {team2.Team}");
            }
            else
            {
                break;
            }
        }

        if (remainingTeams.Count != 0)
        {
            Console.WriteLine("Error: Unable to generate all valid pairs for the quarterfinals.");
            return;
        }

        Console.WriteLine("\nČetvrtfinale mečevi i rezultati:\n");

        // Simulacija i printovanje cetvrtfinala

        List<BasketballTeam> semiFinalists = new List<BasketballTeam>();
        foreach (var match in quarterFinals)
        {
            BasketballTeam winner = SimulateKnockoutMatch(match.Item1, match.Item2, preseasonResults);
            semiFinalists.Add(winner);
        }

        if (semiFinalists.Count < 4)
        {
            Console.WriteLine("Error: Not enough teams for the semifinals.");
            return;
        }

        // Simulacija i printovanje polufinala

        Console.WriteLine("\nPolufinale:");
        List<BasketballTeam> finalists = new List<BasketballTeam>();

        for (int i = 0; i < semiFinalists.Count; i += 2)
        {
            BasketballTeam winner = SimulateKnockoutMatch(semiFinalists[i], semiFinalists[i + 1], preseasonResults);
            finalists.Add(winner);
        }

        if (finalists.Count < 2)
        {
            Console.WriteLine("Error: Not enough teams for the final.");
            return;
        }

        Console.WriteLine("\nMeč za treće mesto:");
        var thirdPlaceTeams = semiFinalists.Except(finalists).ToList();

        if (thirdPlaceTeams.Count < 2)
        {
            Console.WriteLine("Error: Not enough teams for the third-place match.");
            return;
        }

        BasketballTeam thirdPlaceTeam = SimulateKnockoutMatch(thirdPlaceTeams[0], thirdPlaceTeams[1], preseasonResults);

        // Simulacija i printovanje finala

        Console.WriteLine("\nFinale:");
        BasketballTeam champion = SimulateKnockoutMatch(finalists[0], finalists[1], preseasonResults);

        BasketballTeam runnerUp = finalists.First(team => team != champion);

        Console.WriteLine("\nMedalje:\n");

        Console.WriteLine($"1. {champion.Team} ({champion.ISOCode})");
        Console.WriteLine($"2. {runnerUp.Team} ({champion.ISOCode})");
        Console.WriteLine($"3. {thirdPlaceTeam.Team} ({thirdPlaceTeam.ISOCode})");
    }
    
    static bool HavePlayedInGroupStage(BasketballTeam team1, BasketballTeam team2, Dictionary<string, string> teamGroupMembership)
    {
        if (teamGroupMembership.ContainsKey(team1.Team) && teamGroupMembership.ContainsKey(team2.Team))
        {
            return teamGroupMembership[team1.Team] == teamGroupMembership[team2.Team];
        }
        return false;
    }

    // Metoda koja simulira mec eliminacione faze
    static BasketballTeam SimulateKnockoutMatch(BasketballTeam team1, BasketballTeam team2, Dictionary<string, List<MatchResult>> preseasonResults)
    {
        Random random = new Random();

        int rankDifference = Math.Abs(team1.FIBARanking - team2.FIBARanking);

        // Racunanje koeficijenata forme za obe ekipe

        int formCoefficientTeam1 = CalculateFormCoefficient(team1.ISOCode, preseasonResults);
        int formCoefficientTeam2 = CalculateFormCoefficient(team2.ISOCode, preseasonResults);

        double winProbabilityTeam1 = 0.5 + ((formCoefficientTeam1 - formCoefficientTeam2) * 0.05) + 0.01 * (team2.FIBARanking - team1.FIBARanking) * 1.75;

        if(team1.ISOCode == "USA")
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
            team2Points = basePoints - random.Next(1, 5);

            team1Points += rankDifference / 2;
        }
        else
        {
            team2Points = basePoints + random.Next(1, 15);
            team1Points = basePoints - random.Next(1, 5);

            team2Points += rankDifference / 2;
        }

        Console.WriteLine("-------------------------------------------------------");
        Console.WriteLine($"winProbabilityTeam1 - {winProbabilityTeam1}");
        Console.WriteLine($"formCoefficientTeam1 - {formCoefficientTeam1}");
        Console.WriteLine($"formCoefficientTeam2 - {formCoefficientTeam2}");
        Console.WriteLine("-------------------------------------------------------");
        Console.WriteLine($"{team1.Team} - {team2.Team} ({team1Points} : {team2Points})");

        return team1Wins ? team1 : team2;
    }

}
