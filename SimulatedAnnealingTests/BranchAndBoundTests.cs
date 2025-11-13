using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SimulatedAnnealing.Models;
using SimulatedAnnealing.Problems;
using SimulatedAnnealing.Utilities;

namespace SimulatedAnnealingTests
{
    public class BranchAndBoundTests
    {
        private BranchAndBound bnb;
        private List<City> fourCities;
        private List<City> orderedCities;

        [SetUp]
        public void Setup()
        {
            fourCities = new List<City>
            {
                new City {Id = 0, X = 0, Y = 0},
                new City {Id = 1, X = 2, Y = 0},
                new City {Id = 3, X = 0, Y = 2},
                new City {Id = 2, X = 2, Y = 2},
            };

            orderedCities = new List<City>
            {
                new City {Id = 0, X = 0, Y = 0},
                new City {Id = 1, X = 2, Y = 0},
                new City {Id = 2, X = 2, Y = 2},
                new City {Id = 3, X = 0, Y = 2},
            };
        }

        [Test]
        public void ErrorForFourCitiesCalculatedCorrectly()
        {
            // Calculate expected Manhattan distance for the four cities forming a square
            var expectedError = Math.Abs(0 - 2) + Math.Abs(0 - 0) + // City 0 to City 1
                                Math.Abs(2 - 0) + Math.Abs(0 - 2) + // City 1 to City 3
                                Math.Abs(0 - 2) + Math.Abs(2 - 2) + // City 3 to City 2
                                Math.Abs(2 - 0) + Math.Abs(2 - 0);  // City 2 back to City 0

            // Use the DistanceCalculator to verify
            float actualError = 0;
            actualError += DistanceCalculator.CalculateManhattanDistance(fourCities[0], fourCities[1]);
            actualError += DistanceCalculator.CalculateManhattanDistance(fourCities[1], fourCities[2]);
            actualError += DistanceCalculator.CalculateManhattanDistance(fourCities[2], fourCities[3]);
            actualError += DistanceCalculator.CalculateManhattanDistance(fourCities[3], fourCities[0]);

            Assert.AreEqual(expectedError, actualError);
        }

        [Test]
        public async Task BranchAndBoundFindsOptimalRouteAsync()
        {
            bnb = new BranchAndBound(fourCities);
            await bnb.RunAsync();
            var result = bnb.GetBestScenarioFound();

            Assert.IsNotNull(result, "Branch and Bound should return a result");
            Assert.AreEqual(fourCities.Count, result.Count, "Result should contain all cities");
        }

        [Test]
        public void BranchAndBoundFindsOptimalRouteSync()
        {
            bnb = new BranchAndBound(fourCities);
            bnb.Run();
            var result = bnb.GetBestScenarioFound();

            Assert.IsNotNull(result, "Branch and Bound should return a result");
            Assert.AreEqual(fourCities.Count, result.Count, "Result should contain all cities");
        }
    }
}
