using DataPoint;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESportApp;

public static class MatchGenerator
{
    public static DataSeries<Cs2Match> GenerateCs2(string player, int count, int seed = 42)
    {
        var rng = new Random(seed);
        var maps = new[] { "Dust2", "Mirage", "Inferno", "Nuke", "Ancient" };
        var sides = new[] { "CT", "T" };
        var start = new DateTime(2023, 9, 1); // début de la pré-saison

        return DataSeries<Cs2Match>.From(
            Enumerable.Range(1, count)
                .Select(i => new Cs2Match(
                    start.AddDays(i),
                    player,
                    maps[rng.Next(maps.Length)],
                    sides[rng.Next(2)],
                    rng.Next(10, 28),   // kills
                    rng.Next(6, 18),    // deaths
                    rng.Next(0, 8),     // assists
                    rng.Next(0, 5),     // mvps
                    rng.Next(2) == 0    // won
                ))
        );
    }

    public static DataSeries<ValorantMatch> GenerateValorant(string player, int count, int seed = 42)
    {
        var rng = new Random(seed);
        var agents = new[] { "Jett", "Reyna", "Neon", "Omen", "Brimstone", "Astra" };
        var start = new DateTime(2023, 9, 1);

        return DataSeries<ValorantMatch>.From(
            Enumerable.Range(1, count)
                .Select(i => new ValorantMatch(
                    start.AddDays(i),
                    player,
                    agents[rng.Next(agents.Length)],
                    rng.Next(10, 30),   // kills
                    rng.Next(5, 18),    // deaths
                    rng.Next(0, 15),    // assists
                    rng.Next(0, 15),    // headshots
                    rng.Next(5, 14),    // roundsWon
                    rng.Next(2) == 0    // won
                ))
        );
    }

    public static DataSeries<LolMatch> GenerateLol(string player, int count, int seed = 42)
    {
        var rng = new Random(seed);
        var champions = new[] { "Thresh", "Nautilus", "Lulu", "Soraka", "Leona", "Blitzcrank" };
        var start = new DateTime(2023, 9, 1);

        return DataSeries<LolMatch>.From(
            Enumerable.Range(1, count)
                .Select(i => new LolMatch(
                    start.AddDays(i),
                    player,
                    champions[rng.Next(champions.Length)],
                    rng.Next(0, 5),     // kills
                    rng.Next(1, 10),    // deaths
                    rng.Next(10, 25),   // assists
                    rng.Next(20, 60),   // cs
                    rng.Next(40, 80),   // visionScore
                    rng.Next(2) == 0    // won
                ))
        );
    }
}
