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
            Console.WriteLine("Usage: EsportApp [--game valorant|cs2|lol] [--generate <joueur|all>] [--outliers] [--sanitize] [--error strict|soft|hard] [--player <nom>] [--filter wins|losses|all|close] [--kda] [--stat kda|kills|assists] [--normalize] [--smooth <n>]");
            return;
        }

        var valorantPath = ResolveDataPath("valorant.csv");
        var cs2Path = ResolveDataPath("cs2.csv");
        var lolPath = ResolveDataPath("lol.csv");

        var valorant = File.Exists(valorantPath) ? DataSeries<ValorantMatch>.FromCsv(valorantPath, ParseValorant) : null;
        var cs2      = File.Exists(cs2Path) ? DataSeries<Cs2Match>.FromCsv(cs2Path, ParseCs2) : null;
        var lol      = File.Exists(lolPath) ? DataSeries<LolMatch>.FromCsv(lolPath, ParseLol) : null;
        Func<ValorantMatch, bool> isOutlierValorant = m =>
            m.Kills   < 0 || m.Kills > 50 ||
            m.Deaths  < 0 || m.Deaths > 30 ||
            m.Assists < 0;

        Func<Cs2Match, bool> isOutlierCs2 = m =>
            m.Kills + m.Assists > 50 ||
            m.Deaths < 0;

        Func<LolMatch, bool> isOutlierLol = m =>
            m.Kills   > 10 ||
            m.Deaths  < 1  ||
            m.Assists < 0  ||
            m.Cs      < 0;
  
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

        if (args.Contains("--sanitize"))
        {
            if (valorant != null)
            {
                var cleanValorant = valorant.Sanitize(isOutlierValorant);
                Console.WriteLine($"Valorant avant sanitize : {valorant.Count}, après : {cleanValorant.Count}");
            }

            if (cs2 != null)
            {
                var cleanCs2 = cs2.Sanitize(isOutlierCs2);
                Console.WriteLine($"CS2 avant sanitize      : {cs2.Count}, après : {cleanCs2.Count}");
            }

            if (lol != null)
            {
                var cleanLol = lol.Sanitize(isOutlierLol);
                Console.WriteLine($"LoL avant sanitize      : {lol.Count}, après : {cleanLol.Count}");
            }
            return;
        }

        if (args.Contains("--error"))
        {
            var errorIndex = Array.IndexOf(args, "--error") + 1;
            var errorMode = (errorIndex < args.Length ? args[errorIndex] : "soft").ToLower();

            if (errorMode == "strict")
            {
                var vOutliers = valorant?.Outliers(isOutlierValorant);
                var cOutliers = cs2?.Outliers(isOutlierCs2);
                var lOutliers = lol?.Outliers(isOutlierLol);

                int totalOutliers = (vOutliers?.Count ?? 0) + (cOutliers?.Count ?? 0) + (lOutliers?.Count ?? 0);
                if (totalOutliers > 0)
                {
                    Console.WriteLine($"[STRICT] {totalOutliers} erreur(s) détectée(s) :");
                    if (vOutliers != null && vOutliers.Count > 0)
                        Console.WriteLine($"  - Valorant : {vOutliers.Count} outlier(s)");
                    if (cOutliers != null && cOutliers.Count > 0)
                        Console.WriteLine($"  - CS2 : {cOutliers.Count} outlier(s)");
                    if (lOutliers != null && lOutliers.Count > 0)
                        Console.WriteLine($"  - LoL : {lOutliers.Count} outlier(s)");
                    Console.WriteLine("Arrêt immédiat de l'application (mode strict).");
                    return;
                }
                Console.WriteLine("[STRICT] Aucune erreur détectée.");
            }
            else if (errorMode == "soft")
            {
                if (valorant != null) valorant = valorant.Sanitize(isOutlierValorant);
                if (cs2 != null) cs2 = cs2.Sanitize(isOutlierCs2);
                if (lol != null) lol = lol.Sanitize(isOutlierLol);
                Console.WriteLine("[SOFT] Données nettoyées en mémoire.");
            }
            else if (errorMode == "hard")
            {
                if (valorant != null)
                {
                    valorant = valorant.Sanitize(isOutlierValorant);
                    ExportValorant(valorant, valorantPath);
                }
                if (cs2 != null)
                {
                    cs2 = cs2.Sanitize(isOutlierCs2);
                    ExportCs2(cs2, cs2Path);
                }
                if (lol != null)
                {
                    lol = lol.Sanitize(isOutlierLol);
                    ExportLol(lol, lolPath);
                }
                Console.WriteLine("[HARD] Données nettoyées et sauvegardées dans les fichiers CSV.");
            }
            else
            {
                Console.WriteLine($"Mode d'erreur inconnu : '{errorMode}' (modes attendus : strict, soft, hard)");
                return;
            }
        }

    
        string? player = null;
        if (args.Contains("--player"))
        {
            var playerIndex = Array.IndexOf(args, "--player") + 1;
            if (playerIndex < args.Length)
                player = args[playerIndex];
        }

        if (!string.IsNullOrEmpty(player))
        {
            if (valorant != null) valorant = valorant.Filter(m => m.Player.Equals(player, StringComparison.OrdinalIgnoreCase));
            if (cs2 != null) cs2 = cs2.Filter(m => m.Player.Equals(player, StringComparison.OrdinalIgnoreCase));
            if (lol != null) lol = lol.Filter(m => m.Player.Equals(player, StringComparison.OrdinalIgnoreCase));
        }

        string filterMode = "all";
        if (args.Contains("--filter"))
        {
            var filterIndex = Array.IndexOf(args, "--filter") + 1;
            if (filterIndex < args.Length)
                filterMode = args[filterIndex].ToLower();
        }

        var valorantFilters = new Dictionary<string, Func<ValorantMatch, bool>>
        {
            ["wins"]   = m => m.Won,
            ["losses"] = m => !m.Won,
            ["all"]    = m => true,
            ["close"]  = m => Math.Abs(m.RoundsWon - 13) <= 2,
        };

        var cs2Filters = new Dictionary<string, Func<Cs2Match, bool>>
        {
            ["wins"]   = m => m.Won,
            ["losses"] = m => !m.Won,
            ["all"]    = m => true,
            ["close"]  = m => m.Kills + m.Deaths >= 25,
        };

        var lolFilters = new Dictionary<string, Func<LolMatch, bool>>
        {
            ["wins"]   = m => m.Won,
            ["losses"] = m => !m.Won,
            ["all"]    = m => true,
            ["close"]  = m => m.Kills + m.Assists >= 18,
        };

        if (!valorantFilters.ContainsKey(filterMode))
        {
            Console.WriteLine($"Filtre inconnu : '{filterMode}' (modes attendus : wins, losses, all, close)");
            return;
        }

        if (valorant != null) valorant = valorant.Filter(valorantFilters[filterMode]);
        if (cs2 != null) cs2 = cs2.Filter(cs2Filters[filterMode]);
        if (lol != null) lol = lol.Filter(lolFilters[filterMode]);

     
        var valorantSelectors = new Dictionary<string, Func<ValorantMatch, double>>
        {
            ["kda"]     = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
            ["kills"]   = m => m.Kills,
            ["assists"] = m => m.Assists,
            ["deaths"]  = m => m.Deaths,
        };

        var cs2Selectors = new Dictionary<string, Func<Cs2Match, double>>
        {
            ["kda"]     = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
            ["kills"]   = m => m.Kills,
            ["assists"] = m => m.Assists,
            ["deaths"]  = m => m.Deaths,
        };

        var lolSelectors = new Dictionary<string, Func<LolMatch, double>>
        {
            ["kda"]     = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
            ["kills"]   = m => m.Kills,
            ["assists"] = m => m.Assists,
            ["deaths"]  = m => m.Deaths,
            ["cs"]      = m => m.Cs,
        };

   
        string? game = null;
        if (args.Contains("--game"))
        {
            var gameIndex = Array.IndexOf(args, "--game") + 1;
            if (gameIndex < args.Length)
                game = args[gameIndex].ToLower();
        }

        var playerInfo = player != null ? $" pour '{player}'" : "";
        var filterInfo = filterMode != "all" ? $" (filtre: {filterMode})" : "";

  
        bool normalize = args.Contains("--normalize");
        bool hasStat = args.Contains("--stat");
        bool hasKda = args.Contains("--kda");
        string stat = "kda";

        if (hasStat)
        {
            var statIndex = Array.IndexOf(args, "--stat") + 1;
            if (statIndex < args.Length && !args[statIndex].StartsWith("--"))
                stat = args[statIndex].ToLower();
        }

        if (hasStat && !valorantSelectors.ContainsKey(stat))
        {
            Console.WriteLine($"Statistique inconnue : '{stat}' (modes attendus : kda, kills, assists, deaths)");
            return;
        }

        int smoothWindow = 0;
        if (args.Contains("--smooth"))
        {
            var smoothIndex = Array.IndexOf(args, "--smooth") + 1;
            if (smoothIndex >= args.Length || args[smoothIndex].StartsWith("--") || !int.TryParse(args[smoothIndex], out smoothWindow))
            {
                smoothWindow = -1; // valeur non numérique → message d'erreur, pas d'exception
            }
        }

        if (smoothWindow < 0)
        {
            var smoothIndex = Array.IndexOf(args, "--smooth") + 1;
            var raw = smoothIndex < args.Length ? args[smoothIndex] : "";
            Console.WriteLine($"Fenêtre de lissage invalide : '{raw}' (attendu : un entier >= 1)");
            return;
        }

        if (args.Contains("--smooth") && smoothWindow < 1)
        {
            Console.WriteLine("Fenêtre de lissage invalide : la taille minimale est 1.");
            return;
        }

        
        if (normalize && !hasStat && !hasKda && !args.Contains("--smooth") && player == null && filterMode == "all" && game == null)
        {
            var kdaLea     = valorant?.Filter(m => m.Player == "Léa");
            var kdaRaphael = cs2?.Filter(m => m.Player == "Raphaël");
            var kdaNoe     = lol?.Filter(m => m.Player == "Noé");

            var kdaLeaNorm     = kdaLea?.Normalize(valorantSelectors["kda"]);
            var kdaRaphaelNorm = kdaRaphael?.Normalize(cs2Selectors["kda"]);
            var kdaNoeNorm     = kdaNoe?.Normalize(lolSelectors["kda"]);

            Console.WriteLine("=== Comparaison des KDA normalisés dans [0, 1] ===");
            if (kdaLeaNorm != null && kdaLeaNorm.Count > 0)
                Console.WriteLine($"Léa (Valorant)   : min = {kdaLeaNorm.Values.Min():F2}, max = {kdaLeaNorm.Values.Max():F2} -> [{string.Join(", ", kdaLeaNorm.Values.Select(k => k.ToString("F2")))}]");
            if (kdaRaphaelNorm != null && kdaRaphaelNorm.Count > 0)
                Console.WriteLine($"Raphaël (CS2)    : min = {kdaRaphaelNorm.Values.Min():F2}, max = {kdaRaphaelNorm.Values.Max():F2} -> [{string.Join(", ", kdaRaphaelNorm.Values.Select(k => k.ToString("F2")))}]");
            if (kdaNoeNorm != null && kdaNoeNorm.Count > 0)
                Console.WriteLine($"Noé (LoL)        : min = {kdaNoeNorm.Values.Min():F2}, max = {kdaNoeNorm.Values.Max():F2} -> [{string.Join(", ", kdaNoeNorm.Values.Select(k => k.ToString("F2")))}]");
            return;
        }

        // 4.1, 4.2 & 4.3 — Calcul et affichage de stats / normalisation / lissage
        if (hasKda || hasStat || normalize || args.Contains("--smooth"))
        {
            if (hasKda && !normalize && !hasStat && !args.Contains("--smooth") && player == null && filterMode == "all" && game == null)
            {
                if (valorant != null)
                {
                    var kdaAll = valorant.Transform(valorantSelectors["kda"]);
                    var kdaLea = valorant.Filter(m => m.Player == "Léa").Transform(valorantSelectors["kda"]);
                    var kdaWins = valorant.Filter(m => m.Won).Transform(valorantSelectors["kda"]);
                    var kdaLeaWins = valorant.Filter(m => m.Player == "Léa" && m.Won).Transform(valorantSelectors["kda"]);

                    Console.WriteLine("=== KDA Valorant (Transform) ===");
                    Console.WriteLine($"Tous les matchs ({kdaAll.Count}) : moyenne = {kdaAll.Values.Average():F2}");
                    Console.WriteLine($"Matchs de Léa ({kdaLea.Count})   : moyenne = {kdaLea.Values.Average():F2}");
                    Console.WriteLine($"Matchs gagnés ({kdaWins.Count})  : moyenne = {kdaWins.Values.Average():F2}");
                    Console.WriteLine($"Victoires de Léa ({kdaLeaWins.Count}) : moyenne = {kdaLeaWins.Values.Average():F2} -> [{string.Join(", ", kdaLeaWins.Values.Select(k => k.ToString("F2")))}]");
                }

                if (cs2 != null)
                {
                    var kdaRaphael = cs2.Filter(m => m.Player == "Raphaël").Transform(cs2Selectors["kda"]);
                    var kdaKiara = cs2.Filter(m => m.Player == "Kiara").Transform(cs2Selectors["kda"]);

                    Console.WriteLine("\n=== KDA CS2 (Transform) ===");
                    Console.WriteLine($"Raphaël ({kdaRaphael.Count} matchs) : moyenne = {kdaRaphael.Values.Average():F2} -> [{string.Join(", ", kdaRaphael.Values.Select(k => k.ToString("F2")))}]");
                    Console.WriteLine($"Kiara ({kdaKiara.Count} matchs)   : moyenne = {kdaKiara.Values.Average():F2} -> [{string.Join(", ", kdaKiara.Values.Select(k => k.ToString("F2")))}]");
                }

                if (lol != null)
                {
                    var kdaNoe = lol.Filter(m => m.Player == "Noé").Transform(lolSelectors["kda"]);
                    var kdaNoeWins = lol.Filter(m => m.Player == "Noé" && m.Won).Transform(lolSelectors["kda"]);

                    Console.WriteLine("\n=== KDA LoL (Transform) ===");
                    Console.WriteLine($"Noé ({kdaNoe.Count} matchs)       : moyenne = {kdaNoe.Values.Average():F2} -> [{string.Join(", ", kdaNoe.Values.Select(k => k.ToString("F2")))}]");
                    Console.WriteLine($"Noé victoires ({kdaNoeWins.Count} matchs) : moyenne = {kdaNoeWins.Values.Average():F2} -> [{string.Join(", ", kdaNoeWins.Values.Select(k => k.ToString("F2")))}]");
                }
                return;
            }

            void AfficherRapport<TMatch>(string label, DataSeries<TMatch>? serie, Dictionary<string, Func<TMatch, double>> selectors, Func<TMatch, DateTime> getDate, Func<TMatch, string> getPlayer)
            {
                if (serie == null || serie.Count == 0 || !selectors.TryGetValue(stat, out var selecteur))
                    return;

                var retenus = serie;
                DataSeries<double> valeurs = normalize
                    ? retenus.Normalize(selecteur)
                    : retenus.Transform(selecteur);

                if (smoothWindow > 0)
                    valeurs = valeurs.Smooth(v => v, smoothWindow);

                if (smoothWindow > retenus.Count)
                {
                    Console.WriteLine($"{label} : {retenus.Count} matchs, {retenus.Count} retenu(s)");
                    Console.WriteLine($"  fenêtre de lissage ({smoothWindow}) plus large que la série ({retenus.Count}) — rien à afficher\n");
                    return;
                }

                int decalage = smoothWindow > 0 ? smoothWindow - 1 : 0;
                string etiquette = stat.ToLower()
                                 + (normalize ? " normalisé" : "")
                                 + (smoothWindow > 0 ? $" lissé({smoothWindow})" : "");

                if (player != null || args.Contains("--smooth"))
                {
                    Console.WriteLine($"{label}{playerInfo}{filterInfo} : {retenus.Count} match(s) retenu(s)");
                    foreach (var (match, val) in retenus.Values.Skip(decalage).Zip(valeurs.Values))
                    {
                        Console.WriteLine($"  {getDate(match):yyyy-MM-dd}  {getPlayer(match),-8}  {etiquette} = {val:F2}");
                    }
                    Console.WriteLine();
                }
                else
                {
                    var normLabel = normalize ? " (normalisé [0, 1])" : "";
                    Console.WriteLine($"{label}{playerInfo}{filterInfo} {stat.ToUpper()}{normLabel} ({valeurs.Count} matchs) : min = {valeurs.Values.Min():F2}, max = {valeurs.Values.Max():F2}, moy = {valeurs.Values.Average():F2} -> [{string.Join(", ", valeurs.Values.Select(v => v.ToString("F2")))}]");
                }
            }

            if (game == null || game == "all" || game == "valorant")
                AfficherRapport("Valorant", valorant, valorantSelectors, m => m.Timestamp, m => m.Player);
            if (game == null || game == "all" || game == "cs2")
                AfficherRapport("CS2", cs2, cs2Selectors, m => m.Timestamp, m => m.Player);
            if (game == null || game == "all" || game == "lol")
                AfficherRapport("LoL", lol, lolSelectors, m => m.Timestamp, m => m.Player);

            return;
        }

        if (game == null || game == "all" || game == "valorant")
            Console.WriteLine($"Valorant{playerInfo}{filterInfo} : {(valorant != null ? valorant.Count : 0)} matchs");
        if (game == null || game == "all" || game == "cs2")
            Console.WriteLine($"CS2{playerInfo}{filterInfo}      : {(cs2 != null ? cs2.Count : 0)} matchs");
        if (game == null || game == "all" || game == "lol")
            Console.WriteLine($"LoL{playerInfo}{filterInfo}      : {(lol != null ? lol.Count : 0)} matchs");
    }
}
