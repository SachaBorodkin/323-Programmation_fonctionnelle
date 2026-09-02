using DataPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESportApp
{
    public static class MatchGenerator
    {
        public static DataSeries<Cs2Match> GenerateCs2(string player, int count, int seed = 42)
        {
            var rng = new Random(seed);
            var maps = new[] { "Dust2", "Mirage", "Cache", "Nuke", "Ancient", "Inferno", "Train" };
            var sides = new[] { "CT", "T" };
            var start = new DateTime(2023, 9, 1); // début de la pré-saison

            return DataSeries<Cs2Match>.From(
                Enumerable.Range(1, count)
                    .Select(i => new DataPoint<Cs2Match>(
                        start.AddDays(i),
                        new Cs2Match(player, maps[rng.Next(maps.Length)], sides[rng.Next(2)],
                        rng.Next(10, 28),
                        rng.Next(6, 18),
                        rng.Next(0, 8),
                        rng.Next(0, 5),
                        rng.Next(2) == 0)
                    ))
            );
        }

        public static DataSeries<ValorantMatch> GenerateValorant(string player, int count, int seed = 42)
        {
            var rng = new Random(seed);
            var agents = new[] { "Kirk", "Stein", "Big Yahu", "Diddy", "Zeleboba" };
            var start = new DateTime(2023, 9, 1);

            return DataSeries<ValorantMatch>.From(
                Enumerable.Range(1, count)
                    .Select(i => new DataPoint<ValorantMatch>(
                        start.AddDays(i),
                        new ValorantMatch(player, agents[rng.Next(agents.Length)],
                        rng.Next(10, 30),
                        rng.Next(5, 18),
                        rng.Next(0, 15),
                        rng.Next(0, 100),
                        rng.Next(10, 26),
                        rng.Next(2) == 0)
                    ))
            );
        }

        public static DataSeries<LolMatch> GenerateLol(string player, int count, int seed = 42)
        {
            var rng = new Random(seed);
            var champions = new[] { "Jeff", "Le Bron James", "Charlie Chaplin", "IS EP", "Poroshenko" };
            var start = new DateTime(2023, 9, 1);

            return DataSeries<LolMatch>.From(
                Enumerable.Range(1, count)
                    .Select(i => new DataPoint<LolMatch>(
                        start.AddDays(i),
                        new LolMatch(player, champions[rng.Next(champions.Length)],
                        rng.Next(1, 15),
                        rng.Next(0, 12),
                        rng.Next(0, 20),
                        rng.Next(40, 200),
                        rng.Next(50, 200),
                        rng.Next(2) == 0)
                    ))
            );
        }
    }
}
