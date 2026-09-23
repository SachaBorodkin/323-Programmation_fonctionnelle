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

    public static void ShowHelp()
    {
        Console.WriteLine("Usage: EsportApp [options]");
        Console.WriteLine();
        Console.WriteLine("  Analyse des performances de Team Helvetia (Valorant, CS2, LoL).");
        Console.WriteLine();
        Console.WriteLine("Sélection des données");
        Console.WriteLine("  --game   valorant|cs2|lol    Jeu à analyser              (défaut : les trois)");
        Console.WriteLine("  --player <nom>               Restreindre à un joueur     (défaut : tous)");
        Console.WriteLine("  --filter wins|losses|all     Issue des matchs retenus    (défaut : all)");
        Console.WriteLine();
        Console.WriteLine("Analyse");
        Console.WriteLine("  --stat   kda|kills|assists   Indicateur calculé/affiché  (défaut : kda)");
        Console.WriteLine("  --normalize                  Ramène l'indicateur dans [0.0, 1.0]");
        Console.WriteLine("  --smooth <n>                 Moyenne glissante sur n valeurs");
        Console.WriteLine("                                 (normalisation puis lissage, dans cet ordre)");
        Console.WriteLine("  --extract min|max|avg|mme    Extraire un indicateur (avec le détail des matchs)");
        Console.WriteLine();
        Console.WriteLine("Données");
        Console.WriteLine("  --generate <joueur|all>      Simule et exporte les matchs manquants, puis quitte");
        Console.WriteLine("  --error  strict|soft|hard    Traitement des valeurs aberrantes (défaut : soft)");
        Console.WriteLine("                                 strict : les affiche et s'arrête");
        Console.WriteLine("                                 soft   : les élimine et continue");
        Console.WriteLine("                                 hard   : les élimine, sauve le CSV nettoyé, continue");
        Console.WriteLine();
        Console.WriteLine("Divers");
        Console.WriteLine("  --help                       Affiche cette aide");
        Console.WriteLine("  --version                    Affiche la version");
    }

    public static void Main(string[] args)
    {
        if (args.Contains("--version"))
        {
            Console.WriteLine("EsportApp 0.5");
            return;
        }

        if (args.Length == 0 || args.Contains("--help"))
        {
            ShowHelp();
            return;
        }

        var knownFlags = new HashSet<string>
        {
            "--game", "--player", "--filter", "--stat", "--normalize", "--smooth",
            "--generate", "--error", "--help", "--version",
            "--outliers", "--sanitize", "--kda",
            "--extract", "--mme",
        };

        string? unknownFlag = args.FirstOrDefault(a => a.StartsWith("--") && !knownFlags.Contains(a));
        if (unknownFlag != null)
        {
            Console.WriteLine($"Flag inconnu : {unknownFlag}");
            ShowHelp();
            return;
        }

        string? ValueOf(string flag)
        {
            int i = Array.IndexOf(args, flag);
            if (i < 0 || i + 1 >= args.Length || args[i + 1].StartsWith("--"))
                return null;
            return args[i + 1];
        }

        var valueFlags = new[]
        {
            "--game", "--player", "--filter", "--stat", "--smooth", "--generate", "--error",
            "--extract"
        };
        string? flagSansValeur = valueFlags.FirstOrDefault(f => args.Contains(f) && ValueOf(f) == null);
        if (flagSansValeur != null)
        {
            Console.WriteLine($"Le flag {flagSansValeur} attend une valeur.");
            ShowHelp();
            return;
        }

        string? game = ValueOf("--game")?.ToLower();
        var games = new[] { "valorant", "cs2", "lol" };
        if (game != null && !games.Contains(game))
        {
            Console.WriteLine($"Jeu inconnu : {game} (attendu : {string.Join(", ", games)})");
            return;
        }

        string? player = ValueOf("--player");

        string filterMode = (ValueOf("--filter") ?? "all").ToLower();
        var filterModes = new[] { "wins", "losses", "all", "close" };
        if (!filterModes.Contains(filterMode))
        {
            Console.WriteLine($"Filtre inconnu : {filterMode} (attendu : {string.Join(", ", filterModes)})");
            return;
        }

        string stat = (ValueOf("--stat") ?? "kda").ToLower();
        var statNames = new[] { "kda", "kills", "assists" };
        if (args.Contains("--stat") && !statNames.Contains(stat))
        {
            Console.WriteLine($"Stat inconnue : {stat} (attendu : {string.Join(", ", statNames)})");
            return;
        }

        string errorMode = (ValueOf("--error") ?? "soft").ToLower();
        var errorModes = new[] { "strict", "soft", "hard" };
        if (args.Contains("--error") && !errorModes.Contains(errorMode))
        {
            Console.WriteLine($"Mode d'erreur inconnu : {errorMode} (attendu : {string.Join(", ", errorModes)})");
            return;
        }

        string? extractMode = ValueOf("--extract")?.ToLower();
        if (args.Contains("--mme"))
            extractMode = "mme";

        var extractModes = new[] { "min", "max", "avg", "mme" };
        if (extractMode != null && !extractModes.Contains(extractMode))
        {
            Console.WriteLine($"Indicateur à extraire inconnu : {extractMode} (attendu : {string.Join(", ", extractModes)})");
            return;
        }


        int smoothWindow = 0;
        if (args.Contains("--smooth"))
        {
            if (!int.TryParse(ValueOf("--smooth"), out smoothWindow))
                smoothWindow = -1;
        }

        if (smoothWindow < 0)
        {
            Console.WriteLine($"Fenêtre de lissage invalide : {ValueOf("--smooth")} (attendu : un entier >= 1)");
            return;
        }

        if (args.Contains("--smooth") && smoothWindow < 1)
        {
            Console.WriteLine("Fenêtre de lissage invalide : la taille minimale est 1.");
            return;
        }

        if (args.Contains("--generate"))
        {
            string cible = ValueOf("--generate")!;
            GenerateMatches(cible);
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

        var valorantSelectors = new Dictionary<string, Func<ValorantMatch, double>>
        {
            ["kda"]     = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
            ["kills"]   = m => m.Kills,
            ["assists"] = m => m.Assists,
        };

        var cs2Selectors = new Dictionary<string, Func<Cs2Match, double>>
        {
            ["kda"]     = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
            ["kills"]   = m => m.Kills,
            ["assists"] = m => m.Assists,
        };

        var lolSelectors = new Dictionary<string, Func<LolMatch, double>>
        {
            ["kda"]     = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
            ["kills"]   = m => m.Kills,
            ["assists"] = m => m.Assists,
        };

        

        bool normalize = args.Contains("--normalize");

        bool Report<T>(string label,
                       DataSeries<T>? serie,
                       Func<T, bool> estAberrant,
                       Func<T, DateTime> date,
                       Func<T, string> joueur,
                       Func<T, bool> filtre,
                       Func<T, double> statFunc,
                       Action<DataSeries<T>, string> export)
        {
            if (serie == null)
                return true;

            DataSeries<T> aberrants = serie.Outliers(estAberrant);

            if (errorMode == "strict" && aberrants.Count > 0)
            {
                Console.WriteLine($"{label} : {aberrants.Count} valeur(s) aberrante(s) — arrêt (--error strict)");
                foreach (T match in aberrants.Values)
                    Console.WriteLine($"  {match}");
                return false;
            }

            DataSeries<T> propre = serie.Sanitize(estAberrant);

            if (errorMode == "hard")
            {
                string fichier = $"{label.ToLower()}_clean.csv";
                export(propre, fichier);
                Console.WriteLine($"{label} : série nettoyée sauvée dans {fichier}");
            }

            DataSeries<T> retenus = propre
                .Filter(m => player == null || joueur(m).Equals(player, StringComparison.OrdinalIgnoreCase))
                .Filter(filtre);

            Console.WriteLine($"{label} : {serie.Count} matchs, {aberrants.Count} écarté(s), {retenus.Count} retenu(s)");

            if (extractMode != null)
            {
                if (retenus.Count == 0)
                {
                    Console.WriteLine("  aucun match retenu — rien à extraire\n");
                    return true;
                }

                foreach (T match in retenus.Values)
                    Console.WriteLine($"  {date(match):yyyy-MM-dd}  {joueur(match),-8}  {stat} = {statFunc(match):F2}");

                double resultat = extractMode switch
                {
                    "min" => retenus.Values.Select(statFunc).Min(),
                    "max" => retenus.Values.Select(statFunc).Max(),
                    "avg" => retenus.Values.Select(statFunc).Average(),
                    "mme" => retenus.MME(statFunc),
                    _     => throw new InvalidOperationException($"Indicateur inconnu : {extractMode}")
                };

                string libelle = extractMode switch
                {
                    "min" => "Min",
                    "max" => "Max",
                    "avg" => "Moyenne",
                    "mme" => "MME",
                    _     => extractMode
                };
                string suffixe = extractMode == "mme" ? " - forme du moment" : "";

                Console.WriteLine($"  {libelle} ({stat}){suffixe} : {resultat:F2}\n");
                return true;
            }

            DataSeries<double> valeurs = normalize
                ? retenus.Normalize(statFunc)
                : retenus.Transform(statFunc);

            if (smoothWindow > 0)
                valeurs = valeurs.Smooth(v => v, smoothWindow);

            if (smoothWindow > retenus.Count)
            {
                Console.WriteLine($"  fenêtre de lissage ({smoothWindow}) plus large que la série ({retenus.Count}) — rien à afficher\n");
                return true;
            }

            int decalage = smoothWindow > 0 ? smoothWindow - 1 : 0;
            string etiquette = stat
                             + (normalize ? " normalisé" : "")
                             + (smoothWindow > 0 ? $" lissé({smoothWindow})" : "");

            foreach (var (match, valeur) in retenus.Values.Skip(decalage).Zip(valeurs.Values))
                Console.WriteLine($"  {date(match):yyyy-MM-dd}  {joueur(match),-8}  {etiquette} = {valeur:F2}");

            Console.WriteLine();
            return true;
        }

        Console.WriteLine();

        if (game == null || game == "all" || game == "valorant")
            if (!Report("Valorant", valorant, isOutlierValorant,
                        m => m.Timestamp, m => m.Player, valorantFilters[filterMode], valorantSelectors[stat],
                        ExportValorant)) return;

        if (game == null || game == "all" || game == "cs2")
            if (!Report("CS2", cs2, isOutlierCs2,
                        m => m.Timestamp, m => m.Player, cs2Filters[filterMode], cs2Selectors[stat],
                        ExportCs2)) return;

        if (game == null || game == "all" || game == "lol")
            if (!Report("LoL", lol, isOutlierLol,
                        m => m.Timestamp, m => m.Player, lolFilters[filterMode], lolSelectors[stat],
                        ExportLol)) return;
    }
}
