using System;
using System.IO;
using System.Linq;
using DataPoint;

namespace ESportApp;

public class Program
{
    public static ValorantMatch ParseValorant(string[] cols) => new ValorantMatch(
        DateTime.Parse(cols[0]), // timestamp
        cols[1],                 // player
        cols[2],                 // agent
        int.Parse(cols[3]),      // kills
        int.Parse(cols[4]),      // deaths
        int.Parse(cols[5]),      // assists
        int.Parse(cols[6]),      // headshots
        int.Parse(cols[7]),      // roundsWon
        bool.Parse(cols[8])      // won
    );

    public static Cs2Match ParseCs2(string[] cols) => new Cs2Match(
        DateTime.Parse(cols[0]), // timestamp
        cols[1],                 // player
        cols[2],                 // map
        cols[3],                 // startSide (CT ou T)
        int.Parse(cols[4]),      // kills
        int.Parse(cols[5]),      // deaths
        int.Parse(cols[6]),      // assists
        int.Parse(cols[7]),      // mvps
        bool.Parse(cols[8])      // won
    );

    public static LolMatch ParseLol(string[] cols) => new LolMatch(
        DateTime.Parse(cols[0]), // timestamp
        cols[1],                 // player
        cols[2],                 // champion
        int.Parse(cols[4]),      // kills
        int.Parse(cols[5]),      // deaths
        int.Parse(cols[6]),      // assists
        int.Parse(cols[7]),      // cs
        int.Parse(cols[8]),      // visionScore
        bool.Parse(cols[9])      // won
    );

    public static void ExportCs2(DataSeries<Cs2Match> matches, string path)
    {
        var header = "date,player,map,start_side,kills,deaths,assists,mvps,won";
        var lines = matches.Values.Select(m =>
            $"{m.Timestamp:yyyy-MM-dd},{m.Player},{m.Map},{m.StartSide}," +
            $"{m.Kills},{m.Deaths},{m.Assists},{m.Mvps},{m.Won.ToString().ToLower()}"
        );
        File.WriteAllLines(path, lines.Prepend(header));
    }

    public static void ExportValorant(DataSeries<ValorantMatch> matches, string path)
    {
        var header = "date,player,agent,kills,deaths,assists,headshots,rounds_won,won";
        var lines = matches.Values.Select(m =>
            $"{m.Timestamp:yyyy-MM-dd},{m.Player},{m.Agent}," +
            $"{m.Kills},{m.Deaths},{m.Assists},{m.Headshots},{m.RoundsWon},{m.Won.ToString().ToLower()}"
        );
        File.WriteAllLines(path, lines.Prepend(header));
    }

    public static void ExportLol(DataSeries<LolMatch> matches, string path)
    {
        var header = "date,player,champion,kills,deaths,assists,cs,vision_score,won";
        var lines = matches.Values.Select(m =>
            $"{m.Timestamp:yyyy-MM-dd},{m.Player},{m.Champion}," +
            $"{m.Kills},{m.Deaths},{m.Assists},{m.Cs},{m.VisionScore},{m.Won.ToString().ToLower()}"
        );
        File.WriteAllLines(path, lines.Prepend(header));
    }

    public static void GenerateMatches(string target)
    {
        // Prédicats de validation des matchs générés
        Func<Cs2Match, bool> isValidCs2 = m =>
            m.Kills + m.Assists <= 50 &&
            m.Deaths >= 1;

        Func<ValorantMatch, bool> isValidValorant = m =>
            m.Kills + m.Assists <= 45 &&
            m.Deaths >= 1;

        Func<LolMatch, bool> isValidLol = m =>
            m.Deaths >= 1;

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
    }

 
    public static void AskForGeneration()
    {
        Console.WriteLine("\n--- Génération de données (recrues) ---");
        Console.WriteLine("Joueurs disponibles : Raphaël (CS2), Kiara (CS2), Dylan (Valorant), Noé (LoL) ou 'all'");
        Console.Write("Entrez le nom du joueur à générer (défaut: all) : ");
        var input = Console.ReadLine()?.Trim();
        var target = string.IsNullOrWhiteSpace(input) ? "all" : input;
        GenerateMatches(target);
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
      
        if (args.Contains("--generate"))
        {
            var targetIndex = Array.IndexOf(args, "--generate") + 1;
            if (targetIndex < args.Length && !args[targetIndex].StartsWith("--"))
            {
                GenerateMatches(args[targetIndex]);
            }
            else
            {
                AskForGeneration();
            }
            return;
        }

        if (args.Length == 0 || args.Contains("--help"))
        {
            Console.WriteLine("Usage: EsportApp [--game valorant|cs2|lol] [--generate <joueur|all>] [--outliers] [--sanitize]");
            return;
        }

        var valorantPath = ResolveDataPath("valorant.csv");
        var cs2Path = ResolveDataPath("cs2.csv");
        var lolPath = ResolveDataPath("lol.csv");

        var valorant = File.Exists(valorantPath) ? DataSeries<ValorantMatch>.FromCsv(valorantPath, ParseValorant) : null;
        var cs2      = File.Exists(cs2Path) ? DataSeries<Cs2Match>.FromCsv(cs2Path, ParseCs2) : null;
        var lol      = File.Exists(lolPath) ? DataSeries<LolMatch>.FromCsv(lolPath, ParseLol) : null;

  
        if (args.Contains("--outliers"))
        {
            if (valorant != null)
            {
                var baaad = valorant.Outliers(m => m.Kills < 0);
                Console.WriteLine($"Valorant total : {valorant.Count} (inchangé)");
                Console.WriteLine($"Outliers détectés (Kills < 0) : {baaad.Count}");
            }
            return;
        }

        // 3.2 — Supprimer les erreurs avec Sanitize
        if (args.Contains("--sanitize"))
        {
            if (valorant != null)
            {
                var cleanValorant = valorant.Sanitize(m =>
                    m.Kills   < 0 || m.Kills > 50 ||
                    m.Deaths  < 0 || m.Deaths > 30 ||
                    m.Assists < 0
                );
                Console.WriteLine($"Valorant avant sanitize : {valorant.Count}, après : {cleanValorant.Count}");
            }

            if (cs2 != null)
            {
                var cleanCs2 = cs2.Sanitize(m =>
                    m.Kills + m.Assists > 50 ||
                    m.Deaths < 0
                );
                Console.WriteLine($"CS2 avant sanitize      : {cs2.Count}, après : {cleanCs2.Count}");
            }

            if (lol != null)
            {
                var cleanLol = lol.Sanitize(m =>
                    m.Kills   > 10 ||
                    m.Deaths  < 1  ||
                    m.Assists < 0  ||
                    m.Cs      < 0
                );
                Console.WriteLine($"LoL avant sanitize      : {lol.Count}, après : {cleanLol.Count}");
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

        if (game == null || game == "all" || game == "valorant")
            Console.WriteLine($"Valorant : {(valorant != null ? valorant.Count : 0)} matchs");
        if (game == null || game == "all" || game == "cs2")
            Console.WriteLine($"CS2      : {(cs2 != null ? cs2.Count : 0)} matchs");
        if (game == null || game == "all" || game == "lol")
            Console.WriteLine($"LoL      : {(lol != null ? lol.Count : 0)} matchs");
    }
}
