using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPoint
{

    public class DataSeries<T>
    {
        private readonly IEnumerable<DataPoint<T>> _data;
        private DataSeries(IEnumerable<DataPoint<T>> data) => _data = data;
        public static DataSeries<T> From(IEnumerable<DataPoint<T>> source) => new DataSeries<T>(source);
        public IEnumerable<T> Values => _data.Select(dp => dp.Value);   // valeurs seules
        public IEnumerable<DataPoint<T>> DataPoints => _data;
        public int Count => _data.Count();

        public DataSeries<T> Filter(Func<T, bool> predicate)
        {
            return new DataSeries<T>(_data.Where(dp => predicate(dp.Value)));
        }

        public static DataSeries<T> FromCsv(string path, Func<string[], T> parser)
        {
            var lines = File.ReadAllLines(path).Skip(1); // ignorer l'en-tête
            return new DataSeries<T>(lines.Select(line =>
            {
                var cols = line.Split(',');
                return new DataPoint<T>(DateTime.Parse(cols[0]), parser(cols));
            }));
        }
    }
}
