using Basketball_Tournament_Simulation;
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
            Console.WriteLine("--------------OLIMPIJSKE IGRE - KOŠARKA--------------\n");

            Console.WriteLine("Mečevi grupne faze:");

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

                SimulateGroupsStage.SimulateGroupMatches(groupName, teams, preseasonResults);
                SimulateKnockouts.RankTeams(teams);
                SimulateGroupsStage.PrintGroupResults(groupName, teams);
            }

            // Odredjivanje timova koji prolaze u eliminacionu fazu

            List<BasketballTeam> advancingTeams = SimulateKnockouts.RankAdvancingTeams(groups);

            Console.WriteLine("\nTimovi koji prolaze u eliminacionu fazu:");
            foreach (var team in advancingTeams)
            {
                Console.WriteLine($"{team.Team} ({team.ISOCode})");
            }

            Console.WriteLine("\n--------------------------------------------------------------------\n");

            // Perform the draw and simulate knockout stage
            Console.WriteLine("Eliminaciona faza - parovi i mecevi:\n");
            SimulateKnockouts.PerformKnockoutStage(advancingTeams, teamGroupMembership, preseasonResults);
        }
        else
        {
            Console.WriteLine("Error: Unable to read or parse groups.json file.");
        }
    }
}
