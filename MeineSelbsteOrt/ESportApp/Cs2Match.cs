using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESportApp
{
    public class Cs2Match
    {
        public Cs2Match(DateTime timestamp, string player, string map, string startSide, int kills, int deaths, int assists, int mvps, bool won)
        {
            Timestamp = timestamp;
            Player = player;
            Map = map;
            StartSide = startSide;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Mvps = mvps;
            Won = won;
        }

        public Cs2Match(string player, string map, string startSide, int kills, int deaths, int assists, int mvps, bool won)
            : this(default, player, map, startSide, kills, deaths, assists, mvps, won) { }

        public DateTime Timestamp { get; }
        public string Player { get; }
        public string Map { get; }
        public string StartSide { get; }
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Mvps { get; }
        public bool Won { get; }

        public override string ToString()
            => $"Date: {Timestamp:yyyy-MM-dd}, Player: {Player}, Map: {Map}, Start Side: {StartSide}, Kills: {Kills}, Deaths: {Deaths}, Assists: {Assists}, MVPs: {Mvps}, Won: {Won}";
    }
}
