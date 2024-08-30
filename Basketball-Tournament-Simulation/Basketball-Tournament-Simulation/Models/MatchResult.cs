using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basketball_Tournament_Simulation.Models;

public class MatchResult
{
    public string Date { get; set; }
    public string Opponent { get; set; }
    public string Result { get; set; }
    public string OpponentISOCode { get; set; }
}
