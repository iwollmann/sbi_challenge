using SWIChallenge.Models;
using System;
using Xunit;

namespace SWIChallengeTests
{
    public class PredictModelTests
    {
        [Fact]
        public void ValidPointsShouldBeSortedByX() 
        {
            (double x, double y)[] points = new (double, double)[] { (1, 3), (8, 1), (5, 9) };
            var sut = new PredictModel(points);
            sut.Process();

            Assert.Equal(sut.ValidPoints, new (double, double)[] { (1, 3), (5, 9), (8, 1) });
        }

        [Fact]
        public void ValidPointsShouldNotContainsNegativeNumbers() //This is necessary to exponential and power function operations
        {
            (double x, double y)[] points = new (double, double)[] { (-1, 3), (1, -2), (5, 9), (8, 1) };
            var sut = new PredictModel(points);
            sut.Process();

            Assert.Equal(sut.ValidPoints, new (double, double)[] { (5, 9), (8, 1) });
            Assert.Equal(sut.InvalidPoints, new (double, double)[] { (-1, 3), (1, -2) });
        }

        [Fact]
        public void ValidPointsShouldNotContainsZeros() //This is necessary to exponential and power function operations
        {
            (double x, double y)[] points = new (double, double)[] { (0, 3), (1, 0), (5, 9), (8, 1) };
            var sut = new PredictModel(points);
            sut.Process();

            Assert.Equal(sut.ValidPoints, new (double, double)[] { (5, 9), (8, 1) });
            Assert.Equal(sut.InvalidPoints, new (double, double)[] { (0, 3), (1, 0) });
        }

        [Fact]
        public void ValidPointsShouldContainsAtLeastTwoPoints()
        {
            (double x, double y)[] points = new (double, double)[] { (1, 3) };
            var sut = new PredictModel(points);
            sut.Process();

            Assert.False(sut.IsLoaded);
            Assert.True(sut.ValidPoints.Count < 2);
        }

        [Fact]
        public void ShouldBeAbleToAcceptDoubleNumbers()
        {
            (double x, double y)[] points = new (double, double)[] { (1.2, 3.45), (5.2, 1.5642), (2.9, 13.75) };
            var sut = new PredictModel(points);
            sut.Process();

            Assert.Equal(sut.ValidPoints, new (double, double)[] { (1.2, 3.45), (2.9, 13.75), (5.2, 1.5642) });
        }
    }
}
