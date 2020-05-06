using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace SWIChallenge.Models
{
    public class PredictModel
    {
        public readonly IList<(double, double)> Points;
        public IList<(double, double)> InvalidPoints { get; set; }
        public IList<(double, double)> ValidPoints { get; set; }

        public bool IsLoaded { get; set; } = false;

        public PredictModel(IList<(double, double)> points)
        {
            Points = points;
        }

        public string Format((double a, double b) coeff)
            => $"a = {coeff.a:0.##}, b = {coeff.b:0.##}";
        public ((double, double), IEnumerable<(double, double)>) Predict<T>() where T : IFittingModel
        {
            return Activator.CreateInstance<T>().Process(ValidPoints);
        }

        public Task Process()
        {
            ValidPoints = Points
                .Where(x => x.Item1 > 0 && x.Item2 > 0)
                .OrderBy(x => x.Item1)
                .ToList();

            InvalidPoints = Points.Except(ValidPoints).ToList();

            if (ValidPoints.Count >= 2)
                IsLoaded = true;

            return Task.FromResult(true);
        }

        public bool HasInvalidPoints()
            => InvalidPoints.Any();
    }

    public class LinearModel : IFittingModel
    {
        //Linear: y = (a * x) + b
        public ((double, double), IEnumerable<(double, double)>) Process(IEnumerable<(double x, double y)> points)
        {
            //find coefficients
            var (a, b) = MathNet.Numerics.Fit.Line(points.Select(x => x.Item1).ToArray(), points.Select(x => x.Item2).ToArray());
            //calculate Y
            return ((a, b), points.Select(x => (x.Item1, (b * x.Item1) + a)));
        }
    }

    public class ExponentialModel : IFittingModel
    {
        //Exponential: y = a * exp (b * x)
        public ((double, double), IEnumerable<(double, double)>) Process(IEnumerable<(double x, double y)> points)
        {
            //find coefficients
            var (a, b) = MathNet.Numerics.Fit.Exponential(points.Select(x => x.Item1).ToArray(), points.Select(x => x.Item2).ToArray());
            //calculate Y
            return ((a, b), points.Select(x => (x.Item1, (a * (Math.Exp(x.Item1 * b))))));
        }
    }

    public class PowerFunctionModel : IFittingModel
    {
        //Power function: y = a * (x ^ b)
        //y : x -> a*x^b
        public ((double, double), IEnumerable<(double, double)>) Process(IEnumerable<(double x, double y)> points)
        {
            //find coefficients
            var (a, b) = MathNet.Numerics.Fit.Power(points.Select(x => x.Item1).ToArray(), points.Select(x => x.Item2).ToArray());
            //calculate Y
            return ((a, b), points.Select(x => (x.Item1, (a * (Math.Pow(x.Item1, b))))));
        }
    }

    public interface IFittingModel
    {
        ((double, double), IEnumerable<(double, double)>) Process(IEnumerable<(double x, double y)> points);
    }
}
