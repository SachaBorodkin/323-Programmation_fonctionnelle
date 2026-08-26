
using DataSerie;
namespace ESportApp;
public class Program
{
    public static void Main(string[] args)
    {

        var valorantMatches = new[]
        {
            new ValorantMatch("Léa", "Jett",  18, 6, 4, 8,  13, true),
            new ValorantMatch("Léa", "Reyna", 22, 8, 2, 11,  9, false),
            new ValorantMatch("Léa", "Neon",  20, 7, 5,  9, 13, true),
        };

        var valorant = DataSeries<ValorantMatch>.From(valorantMatches);
        var cs2 = DataSeries<Cs2Match>.From(new[]
{
    new Cs2Match("Raphaël", "Mirage",  "CT", 21, 14, 5, 2, true),
    new Cs2Match("Kiara",   "Dust2",   "T",  26, 11, 1, 4, true),
    new Cs2Match("Raphaël", "Inferno", "T",  14, 16, 6, 1, false),
});

        var lol = DataSeries<LolMatch>.From(new[]
        {
    new LolMatch("Noé", "Thresh", 2, 4, 18, 42, 71, true),
    new LolMatch("Noé", "Thresh", 1, 6, 12, 35, 64, false),
});
        if (args.Length == 0 || args.Contains("--help"))
        {
            Console.WriteLine("Usage: EsportApp [--game valorant|cs2|lol]");
            return;
        }

        string? game = null;
        if (args.Contains("--game"))
            game = args[Array.IndexOf(args, "--game") + 1];

        // séries construites avec les matchs en dur des étapes 2 et 3
        if (game == null || game == "valorant")
            Console.WriteLine($"Valorant : {valorant.Count} matchs");
        if (game == null || game == "cs2")
            Console.WriteLine($"CS2      : {cs2.Count} matchs");
        if (game == null || game == "lol")
            Console.WriteLine($"LoL      : {lol.Count} matchs");
    }
}
