using Basketball_Tournament_Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basketball_Tournament_Simulation;

public class FormCoefficient
{
    public static int CalculateFormCoefficient(string isoCode, Dictionary<string, List<MatchResult>> preseasonResults, int tournamentWins)
    {
        int preseasonWins = 0;

        if (preseasonResults.TryGetValue(isoCode, out var matches))
        {
            preseasonWins = matches.Count(match =>
            {
                var scores = match.Result.Split('-').Select(int.Parse).ToArray();
                return scores[0] > scores[1];
            });
        }

        return preseasonWins + tournamentWins; // Zbir predsezonskih i pobeda na turniru zbog pracenja forme
    }
}
