
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

    public static void Main(string[] args)
    {
        var valorant = DataSeries<ValorantMatch>.FromCsv("data/valorant.csv", ParseValorant);
        var cs2      = DataSeries<Cs2Match>.FromCsv("data/cs2.csv", ParseCs2);
        var lol      = DataSeries<LolMatch>.FromCsv("data/lol.csv", ParseLol);

        Console.WriteLine($"Valorant : {valorant.Count} matchs");
        Console.WriteLine($"CS2      : {cs2.Count} matchs");
        Console.WriteLine($"LoL      : {lol.Count} matchs");
        var simpleGenerated = MatchGenerator.GenerateCs2("S1mple", 20);
        Console.WriteLine(simpleGenerated.Count); // 20
        var tmasterGenerated = MatchGenerator.GenerateValorant("Trashmaster", 67);
        Console.WriteLine(tmasterGenerated.Count); // 67
        var KyellGenerated = MatchGenerator.GenerateLol("Kyell Cornu", 67);
        Console.WriteLine(KyellGenerated.Count); // 67

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

        string? game = null;
        if (args.Contains("--game"))
            game = args[Array.IndexOf(args, "--game") + 1];
    }
}
