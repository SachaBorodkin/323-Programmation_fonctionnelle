using System;
using System.IO;
using System.Linq;
using DataPoint;

namespace ESportApp;

public class Program
{
    public static ValorantMatch ParseValorant(string[] cols) => new ValorantMatch(
        cols[1],             
        cols[2],              
        int.Parse(cols[3]),   
        int.Parse(cols[4]),   
        int.Parse(cols[5]),   
        int.Parse(cols[6]),   
        int.Parse(cols[7]),   
        bool.Parse(cols[8])   
    );

    public static Cs2Match ParseCs2(string[] cols) => new Cs2Match(
        cols[1],             
        cols[2],             
        cols[3],             
        int.Parse(cols[4]),   
        int.Parse(cols[5]),   
        int.Parse(cols[6]),   
        int.Parse(cols[7]),   
        bool.Parse(cols[8])   
    );

    public static LolMatch ParseLol(string[] cols) => new LolMatch(
        cols[1],              
        cols[2],             
        int.Parse(cols[4]),   
        int.Parse(cols[5]),   
        int.Parse(cols[6]),   
        int.Parse(cols[7]),  
        int.Parse(cols[8]),   
        bool.Parse(cols[9])   
    );

    public static void ExportCs2(DataSeries<Cs2Match> matches, string path)
    {
        var header = "date,player,map,start_side,kills,deaths,assists,mvps,won";
        var lines = matches.DataPoints.Select(dp =>
            $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Map},{dp.Value.StartSide}," +
            $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Mvps},{dp.Value.Won.ToString().ToLower()}"
        );
        File.WriteAllLines(path, lines.Prepend(header));
    }

    public static void ExportValorant(DataSeries<ValorantMatch> matches, string path)
    {
        var header = "date,player,agent,kills,deaths,assists,headshots,rounds_won,won";
        var lines = matches.DataPoints.Select(dp =>
            $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Agent}," +
            $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Headshots},{dp.Value.RoundsWon},{dp.Value.Won.ToString().ToLower()}"
        );
        File.WriteAllLines(path, lines.Prepend(header));
    }

    public static void ExportLol(DataSeries<LolMatch> matches, string path)
    {
        var header = "date,player,champion,kills,deaths,assists,cs,vision_score,won";
        var lines = matches.DataPoints.Select(dp =>
            $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Champion}," +
            $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Cs},{dp.Value.VisionScore},{dp.Value.Won.ToString().ToLower()}"
        );
        File.WriteAllLines(path, lines.Prepend(header));
    }

    private static string ResolveDataPath(string fileName)
    {
        var direct = Path.Combine("data", fileName);
        if (File.Exists(direct)) return direct;
        var inBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", fileName);
        if (File.Exists(inBase)) return inBase;
        return direct;
    }

    public static void Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help"))
        {
            Console.WriteLine("Usage: EsportApp [--game valorant|cs2|lol] [--generate <joueur|all>]");
            return;
        }

        Func<Cs2Match, bool> isValidCs2 = m =>
            m.Kills + m.Assists <= 50 &&
            m.Deaths >= 1;

        Func<ValorantMatch, bool> isValidValorant = m =>
            m.Kills + m.Assists <= 45 &&
            m.Deaths >= 1;

        Func<LolMatch, bool> isValidLol = m =>
            m.Deaths >= 1;

        if (args.Contains("--generate"))
        {
            var targetIndex = Array.IndexOf(args, "--generate") + 1;
            if (targetIndex >= args.Length)
            {
                Console.WriteLine("Erreur : paramètre manquant pour --generate");
                return;
            }

            var target = args[targetIndex];
            var players = target.Equals("all", StringComparison.OrdinalIgnoreCase)
                ? new[] { "Raphaël", "Kiara", "Dylan", "Noé" }
                : new[] { target };

            foreach (var player in players)
            {
                if (player.Equals("Raphaël", StringComparison.OrdinalIgnoreCase) || player.Equals("Raphael", StringComparison.OrdinalIgnoreCase))
                {
                    var series = MatchGenerator.GenerateCs2("Raphaël", 20, seed: 42);
                    var valid = series.Filter(isValidCs2);
                    ExportCs2(valid, "raphaël_generated.csv");
                    Console.WriteLine($"{player} : données générées et exportées ({valid.Count}/{series.Count} valides)");
                }
                else if (player.Equals("Kiara", StringComparison.OrdinalIgnoreCase))
                {
                    var series = MatchGenerator.GenerateCs2("Kiara", 20, seed: 7);
                    var valid = series.Filter(isValidCs2);
                    ExportCs2(valid, "kiara_generated.csv");
                    Console.WriteLine($"{player} : données générées et exportées ({valid.Count}/{series.Count} valides)");
                }
                else if (player.Equals("Dylan", StringComparison.OrdinalIgnoreCase))
                {
                    var series = MatchGenerator.GenerateValorant("Dylan", 20, seed: 42);
                    var valid = series.Filter(isValidValorant);
                    ExportValorant(valid, "dylan_generated.csv");
                    Console.WriteLine($"{player} : données générées et exportées ({valid.Count}/{series.Count} valides)");
                }
                else if (player.Equals("Noé", StringComparison.OrdinalIgnoreCase) || player.Equals("Noe", StringComparison.OrdinalIgnoreCase))
                {
                    var series = MatchGenerator.GenerateLol("Noé", 20, seed: 42);
                    var valid = series.Filter(isValidLol);
                    ExportLol(valid, "noé_generated.csv");
                    Console.WriteLine($"{player} : données générées et exportées ({valid.Count}/{series.Count} valides)");
                }
                else
                {
                    var series = MatchGenerator.GenerateCs2(player, 20);
                    var valid = series.Filter(isValidCs2);
                    ExportCs2(valid, $"{player.ToLower()}_generated.csv");
                    Console.WriteLine($"{player} : données générées et exportées");
                }
            }
            return;
        }

        // Gestion du flag --game
        string? game = null;
        if (args.Contains("--game"))
        {
            var gameIndex = Array.IndexOf(args, "--game") + 1;
            if (gameIndex < args.Length)
                game = args[gameIndex].ToLower();
        }

        var valorantPath = ResolveDataPath("valorant.csv");
        var cs2Path = ResolveDataPath("cs2.csv");
        var lolPath = ResolveDataPath("lol.csv");

        var valorant = File.Exists(valorantPath) ? DataSeries<ValorantMatch>.FromCsv(valorantPath, ParseValorant) : null;
        var cs2      = File.Exists(cs2Path) ? DataSeries<Cs2Match>.FromCsv(cs2Path, ParseCs2) : null;
        var lol      = File.Exists(lolPath) ? DataSeries<LolMatch>.FromCsv(lolPath, ParseLol) : null;

        if (game == null || game == "all" || game == "valorant")
            Console.WriteLine($"Valorant : {(valorant != null ? valorant.Count : 0)} matchs");
        if (game == null || game == "all" || game == "cs2")
            Console.WriteLine($"CS2      : {(cs2 != null ? cs2.Count : 0)} matchs");
        if (game == null || game == "all" || game == "lol")
            Console.WriteLine($"LoL      : {(lol != null ? lol.Count : 0)} matchs");
    }
}
