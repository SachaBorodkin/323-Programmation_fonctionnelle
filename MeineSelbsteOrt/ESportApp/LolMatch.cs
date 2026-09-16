using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESportApp
{
    public class LolMatch
    {
        public LolMatch(DateTime timestamp, string player, string champion, int kills, int deaths, int assists, int cs, int visionScore, bool won)
        {
            Timestamp = timestamp;
            Player = player;
            Champion = champion;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Cs = cs;
            VisionScore = visionScore;
            Won = won;
        }

        public LolMatch(string player, string champion, int kills, int deaths, int assists, int cs, int visionScore, bool won)
            : this(default, player, champion, kills, deaths, assists, cs, visionScore, won) { }

        public DateTime Timestamp { get; }
        public string Player { get; }
        public string Champion { get; }
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Cs { get; }
        public int VisionScore { get; }
        public bool Won { get; }
    }
}
