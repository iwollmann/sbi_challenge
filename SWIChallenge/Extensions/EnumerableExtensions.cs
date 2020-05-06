using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OxyPlot;
using OxyPlot.Series;

namespace SWIChallenge.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<DataPoint> ToDataPoints(this IEnumerable<(double, double)> points)
            => points.Select(x => new DataPoint(x.Item1, x.Item2));

        public static IEnumerable<ScatterPoint> ToScatterPoints(this IEnumerable<(double, double)> points)
            => points.Select(x => new ScatterPoint(x.Item1, x.Item2));

        public static void Deconstruct<T>(this IList<T> items, out T first, out T second)
        {
            first = items.First();
            second = items.Skip(1).First();
        }
    }
}
