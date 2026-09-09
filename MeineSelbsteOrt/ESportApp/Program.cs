
using DataPoint;
namespace ESportApp;
public class Program
{
    static ValorantMatch ParseValorant(string[] cols) => new ValorantMatch(
        cols[1],
        cols[2],
        int.Parse(cols[3]),
        int.Parse(cols[4]),
        int.Parse(cols[5]),
        int.Parse(cols[6]),
        int.Parse(cols[7]),
        bool.Parse(cols[8])
    );

    static Cs2Match ParseCs2(string[] cols) => new Cs2Match(
        cols[1],
        cols[2],
        cols[3],
        int.Parse(cols[4]),
        int.Parse(cols[5]),
        int.Parse(cols[6]),
        int.Parse(cols[7]),
        bool.Parse(cols[8])
    );

    static LolMatch ParseLol(string[] cols) => new LolMatch(
        cols[1],
        cols[2],
        int.Parse(cols[4]),
        int.Parse(cols[5]),
        int.Parse(cols[6]),
        int.Parse(cols[7]),
        int.Parse(cols[8]),
        bool.Parse(cols[9])
    );

    static void ExportCs2(DataSeries<Cs2Match> matches, string path)
    {
        var header = "date,player,map,start_side,kills,deaths,assists,mvps,won";
        var lines = matches.DataPoints.Select(dp =>
            $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Map},{dp.Value.StartSide}," +
            $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Mvps},{dp.Value.Won.ToString().ToLower()}"
        );
        File.WriteAllLines(path, lines.Prepend(header));
    }

    static void ExportValorant(DataSeries<ValorantMatch> matches, string path)
    {
        var header = "date,player,agent,kills,deaths,assists,headshots,rounds_won,won";
        var lines = matches.DataPoints.Select(dp =>
            $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Agent}," +
            $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Headshots},{dp.Value.RoundsWon},{dp.Value.Won.ToString().ToLower()}"
        );
        File.WriteAllLines(path, lines.Prepend(header));
    }

    static void ExportLol(DataSeries<LolMatch> matches, string path)
    {
        var header = "date,player,champion,kills,deaths,assists,cs,vision_score,won";
        var lines = matches.DataPoints.Select(dp =>
            $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Champion}," +
            $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Cs},{dp.Value.VisionScore},{dp.Value.Won.ToString().ToLower()}"
        );
        File.WriteAllLines(path, lines.Prepend(header));
    }

    public static void Main(string[] args)
    {
        var valorant = DataSeries<ValorantMatch>.FromCsv("data/valorant.csv", ParseValorant);
        var cs2      = DataSeries<Cs2Match>.FromCsv("data/cs2.csv", ParseCs2);
        var lol      = DataSeries<LolMatch>.FromCsv("data/lol.csv", ParseLol);

        Console.WriteLine($"Valorant : {valorant.Count} matchs");
        Console.WriteLine($"CS2      : {cs2.Count} matchs");
        Console.WriteLine($"LoL      : {lol.Count} matchs");
        var simpleGenerated = MatchGenerator.GenerateCs2("S1mple", 20);
        Console.WriteLine("S1mple a joué" + simpleGenerated.Count + "matches"); // 20
        var tmasterGenerated = MatchGenerator.GenerateValorant("Trashmaster", 67);
        Console.WriteLine("TrashMaster a joué" + tmasterGenerated.Count + "matches"); // 67
        var KyellGenerated = MatchGenerator.GenerateLol("Kyell Cornu", 67);
        Console.WriteLine("Kyell a joué "+KyellGenerated.Count+ "matches"); // 67

        Func<Cs2Match, bool> isValidCs2 = m =>
            m.Kills + m.Assists <= 50 &&
            m.Deaths >= 1;
        var simpleValid = simpleGenerated.Filter(isValidCs2);
        Console.WriteLine($"Avant : {simpleGenerated.Count}, après : {simpleValid.Count}");

        Func<ValorantMatch, bool> isValidValorant = m =>
            m.Kills + m.Assists <= 45 &&
            m.Deaths >= 1 &&
            m.Headshots <= 100;

        var trashmasterGenerated = MatchGenerator.GenerateValorant("Trashmaster", 50);
        var trashmasterValid = trashmasterGenerated.Filter(isValidValorant);
        Console.WriteLine($"Avant : {trashmasterGenerated.Count}, après : {trashmasterValid.Count}");

        Func<LolMatch, bool> isValidLol = m =>
            m.Kills + m.Assists <= 25 &&
            m.Deaths >= 1 &&
            m.Cs <= 200;

        var kyellGenerated = MatchGenerator.GenerateLol("Kyell Cornu", 50);
        var kyellValid = kyellGenerated.Filter(isValidLol);
        Console.WriteLine($"Avant : {kyellGenerated.Count}, après : {kyellValid.Count}");
        if (args.Length == 0 || args.Contains("--help"))
        {
            Console.WriteLine("Usage: EsportApp [--game valorant|cs2|lol]");
            return;
        }
        string? game = null;
        if (args.Contains("--game"))
            game = args[Array.IndexOf(args, "--game") + 1];
        if (args.Contains("--generate"))
        {
            var target = args[Array.IndexOf(args, "--generate") + 1];

            var players = target == "all"
                ? new[] { "Raphaël", "Kiara", "Dylan", "Noé" }
                : new[] { target };

            foreach (var player in players)
            {
                var series = MatchGenerator.GenerateCs2(player, 20);
                ExportCs2(series/*.Filter(isValid)*/, $"{player.ToLower()}_generated.csv");
                Console.WriteLine($"{player} : données générées et exportées");
            }
            return;
        }
        Directory.CreateDirectory("data");
        ExportCs2(simpleValid, "data/s1mple_generated_cs2.csv");
        ExportLol(kyellValid, "data/kyell_generated_lol.csv");
        ExportValorant(trashmasterValid, "data/trashmaster_generated_valorant.csv");
    }
}
