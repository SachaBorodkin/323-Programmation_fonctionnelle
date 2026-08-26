using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESportApp
{
    public class Cs2Match
    {
        public Cs2Match(string player, string map, string startSide, int kills, int deaths, int assists, int mvps, bool won)
        {
            Player = player;
            Map = map;
            StartSide = startSide;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Mvps = mvps;
            Won = won;
        }

        public string Player { get; }
        public string Map { get; }
        public string StartSide { get; }
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Mvps { get; }
        public bool Won { get; }
    }
}
