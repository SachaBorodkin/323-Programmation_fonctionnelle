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
    public DataSeries<double> Smooth(Func<T, double> evaluator, int windowSize)
    {
        if (windowSize <= 0)
            return DataSeries<double>.From(Enumerable.Empty<double>());

        var valeurs = _data.Select(evaluator).ToList();
        int combien = Math.Max(0, valeurs.Count - windowSize + 1);

        return DataSeries<double>.From(
            Enumerable.Range(0, combien)
                .Select(debut => valeurs.Skip(debut).Take(windowSize).Average())
                .ToList()
        );
    }

    public double Minimum(Func<T, double> value)
    {
        var valeurs = _data.Select(value).ToList();
        return valeurs.Count > 0 ? valeurs.Min() : 0;
    }
    public double Maximum(Func<T, double> value)
    {
        var valeurs = _data.Select(value).ToList();
        return valeurs.Count > 0 ? valeurs.Max() : 0;
    }
    public double Average(Func<T, double> value)
    {
        var valeurs = _data.Select(value).ToList();
        return valeurs.Count > 0 ? valeurs.Average() : 0;
    }

    public double MME(Func<T, double> value)
    {
        var valeurs = _data.Select(value).ToList();
        if (valeurs.Count == 0) return 0;
        if (valeurs.Count == 1) return valeurs[0];

        return valeurs.Aggregate(valeurs[0], (mme, v) => (v + mme) / 2);
    }

    public TResult Fold<TResult>(TResult seed, Func<TResult, T, TResult> combiner)
        => _data.Aggregate(seed, combiner);

    public IEnumerable<DataSeries<T>> SlidingWindow(int size)
    {
        if (size <= 0) return Enumerable.Empty<DataSeries<T>>();
        var values = _data.ToList();
        return Enumerable.Range(0, Math.Max(0, values.Count - size + 1))
            .Select(i => DataSeries<T>.From(values.Skip(i).Take(size)));
    }

    public SeriesStats Statistics()
    {
        var values = _data.Cast<double>().ToList();
        if (values.Count == 0)
            return new SeriesStats(0, 0, 0, 0);

        var mean = values.Aggregate(0.0, (acc, v) => acc + v) / values.Count;
        var variance = values.Aggregate(0.0, (acc, v) => acc + Math.Pow(v - mean, 2)) / values.Count;
        return new SeriesStats(
            min: values.Min(),
            max: values.Max(),
            mean: mean,
            stdDev: Math.Sqrt(variance)
        );
    }

    public SeriesStats Statistics(Func<T, double> evaluator)
        => Transform(evaluator).Statistics();
}

public class SeriesStats
{
    public double Min { get; }
    public double Max { get; }
    public double Mean { get; }
    public double StdDev { get; }

    public SeriesStats(double min, double max, double mean, double stdDev)
    {
        Min = min;
        Max = max;
        Mean = mean;
        StdDev = stdDev;
    }
}

public static class DataSeriesExtensions
{
    public static DataSeries<double> Smooth(this DataSeries<double> series, int windowSize)
        => series.Smooth(v => v, windowSize);
}
