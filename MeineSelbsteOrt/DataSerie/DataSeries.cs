using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataPoint;

public class DataSeries<T>
{
    private readonly IEnumerable<T> _data;

    private DataSeries(IEnumerable<T> data) => _data = data;

    public static DataSeries<T> From(IEnumerable<T> source)
        => new DataSeries<T>(source);

    public int Count => _data.Count();

    public IEnumerable<T> Values => _data;

    public static DataSeries<T> FromCsv(string path, Func<string[], T> parser)
    {
        var lines = File.ReadAllLines(path).Skip(1); 
        return new DataSeries<T>(lines.Select(line =>
        {
            var cols = line.Split(',');
            return parser(cols);
        }));
    }

    public DataSeries<T> Filter(Func<T, bool> predicate)
        => new DataSeries<T>(_data.Where(predicate));

    public DataSeries<T> Outliers(Func<T, bool> predicate)
        => DataSeries<T>.From(_data.Where(predicate));

    public DataSeries<T> Sanitize(Func<T, bool> isOutlier)
        => DataSeries<T>.From(_data.Where(item => !isOutlier(item)));

    public DataSeries<TResult> Transform<TResult>(Func<T, TResult> mapper)
        => DataSeries<TResult>.From(_data.Select(mapper));

    /// <summary>
    /// 4.2 Évalue chaque objet de la série avec l'outil (fonction) d'évaluation fourni,
    /// et retourne une nouvelle série de valeurs normalisées dans [0, 1].
    /// </summary>
    public DataSeries<double> Normalize(Func<T, double> evaluator)
    {
        var values = _data.Select(evaluator).ToList();
        if (values.Count == 0)
            return DataSeries<double>.From(Enumerable.Empty<double>());

        var min = values.Min();
        var max = values.Max();

        if (Math.Abs(max - min) < 1e-9)
            return DataSeries<double>.From(values.Select(_ => 0.0));

        return DataSeries<double>.From(values.Select(v => (v - min) / (max - min)));
    }
}
