using Basketball_Tournament_Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basketball_Tournament_Simulation;

public class SimulateKnockouts
{
    public static List<BasketballTeam> RankAdvancingTeams(Dictionary<string, List<BasketballTeam>> groups)
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

    public static void RankTeams(List<BasketballTeam> teams)
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

    public static void PerformKnockoutStage(
    List<BasketballTeam> advancingTeams,
    Dictionary<string, string> teamGroupMembership,
    Dictionary<string, List<MatchResult>> preseasonResults)
    {
        Random random = new Random();

        // Kreiranje sesira na osnovu rankinga
        List<BasketballTeam> potD = advancingTeams.Take(2).ToList();
        List<BasketballTeam> potE = advancingTeams.Skip(2).Take(2).ToList();
        List<BasketballTeam> potF = advancingTeams.Skip(4).Take(2).ToList();
        List<BasketballTeam> potG = advancingTeams.Skip(6).Take(2).ToList();

        Console.WriteLine("Šeširi:\n");
        PrintPot("Šešir D:", potD);
        PrintPot("Šešir E:", potE);
        PrintPot("Šešir F:", potF);
        PrintPot("Šešir G:", potG);

        var quarterFinals = new List<Tuple<BasketballTeam, BasketballTeam>>();

        Console.WriteLine("\n--------------------------------------------------------------------\n");

        Console.WriteLine("Četvrtfinale parovi:\n");

        // Match teams from pot D with teams from pot G
        var potDAndGMatches = CreateMatchPairs(potD, potG, teamGroupMembership);
        quarterFinals.AddRange(potDAndGMatches);

        // Match teams from pot E with teams from pot F
        var potEAndFMatches = CreateMatchPairs(potE, potF, teamGroupMembership);
        quarterFinals.AddRange(potEAndFMatches);
        foreach (var match in quarterFinals)
        {
            Console.WriteLine($"{match.Item1.Team} vs {match.Item2.Team}");
        }

        Console.WriteLine("\nČetvrtfinale mečevi i rezultati:\n");

        // Simulate and print quarterfinal matches
        List<BasketballTeam> semiFinalists = new List<BasketballTeam>();
        foreach (var match in quarterFinals)
        {
            BasketballTeam winner = KnockoutMatchSimulation.SimulateKnockoutMatch(match.Item1, match.Item2, preseasonResults);
            semiFinalists.Add(winner);
        }

        if (semiFinalists.Count < 4)
        {
            Console.WriteLine("Greška: Nema dovoljno timova za polufinale.");
            return;
        }

        // Simulate and print semifinals
        Console.WriteLine("\nPolufinale:");
        List<BasketballTeam> finalists = new List<BasketballTeam>();
        for (int i = 0; i < semiFinalists.Count; i += 2)
        {
            BasketballTeam winner = KnockoutMatchSimulation.SimulateKnockoutMatch(semiFinalists[i], semiFinalists[i + 1], preseasonResults);
            finalists.Add(winner);
        }

        if (finalists.Count < 2)
        {
            Console.WriteLine("Greška: Nema dovoljno timova za finale.");
            return;
        }

        // Determine third-place match
        Console.WriteLine("\nMeč za treće mesto:");
        var thirdPlaceTeams = semiFinalists.Except(finalists).ToList();
        if (thirdPlaceTeams.Count < 2)
        {
            Console.WriteLine("Greška: Nema dovoljno timova za meč za treće mesto.");
            return;
        }
        BasketballTeam thirdPlaceTeam = KnockoutMatchSimulation.SimulateKnockoutMatch(thirdPlaceTeams[0], thirdPlaceTeams[1], preseasonResults);

        // Simulate and print the final match
        Console.WriteLine("\nFinale:");
        BasketballTeam champion = KnockoutMatchSimulation.SimulateKnockoutMatch(finalists[0], finalists[1], preseasonResults);
        BasketballTeam runnerUp = finalists.First(team => team != champion);

        Console.WriteLine("\nMedalje:\n");
        Console.WriteLine($"1. {champion.Team} ({champion.ISOCode})");
        Console.WriteLine($"2. {runnerUp.Team} ({runnerUp.ISOCode})");
        Console.WriteLine($"3. {thirdPlaceTeam.Team} ({thirdPlaceTeam.ISOCode})");
    }

    private static void PrintPot(string potName, List<BasketballTeam> teams)
    {
        Console.WriteLine(potName);
        foreach (var team in teams)
        {
            Console.WriteLine($"{team.Team} ({team.ISOCode})");
        }
        Console.WriteLine();
    }

    private static List<Tuple<BasketballTeam, BasketballTeam>> CreateMatchPairs(
        List<BasketballTeam> potA,
        List<BasketballTeam> potB,
        Dictionary<string, string> teamGroupMembership)
    {
        var matches = new List<Tuple<BasketballTeam, BasketballTeam>>();
        var potBUsed = new HashSet<BasketballTeam>();

        foreach (var teamA in potA)
        {
            BasketballTeam teamB = potB.FirstOrDefault(t => !potBUsed.Contains(t) && !HavePlayedInGroupStage(teamA, t, teamGroupMembership));

            if (teamB == null)
            {
                teamB = potB.FirstOrDefault(t => !potBUsed.Contains(t));
            }

            if (teamB != null)
            {
                matches.Add(Tuple.Create(teamA, teamB));
                potBUsed.Add(teamB);
            }
            else
            {
                Console.WriteLine("Greška: Nema dostupnih timova za formiranje parova.");
                break;
            }
        }

        return matches;
    }

    static bool HavePlayedInGroupStage(BasketballTeam team1, BasketballTeam team2, Dictionary<string, string> teamGroupMembership)
    {
        if (teamGroupMembership.ContainsKey(team1.Team) && teamGroupMembership.ContainsKey(team2.Team))
        {
            return teamGroupMembership[team1.Team] == teamGroupMembership[team2.Team];
        }
        return false;
    }
}
