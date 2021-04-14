using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SimulatedAnnealing;

namespace SimulatedAnnealingTests
{
    public class Tests
    {
        private BranchAndBound bnb;
        private List<City> fourCities;
        private List<City> orderedCities;

        [SetUp]
        public async Task Setup()
        {
            fourCities = new List<City>
            {
                new City {OriginalCityNumber = 0, X = 0, Y = 0},
                new City {OriginalCityNumber = 1, X = 2, Y = 0},
                new City {OriginalCityNumber = 3, X = 0, Y = 2},
                new City {OriginalCityNumber = 2, X = 2, Y = 2},
            };

            orderedCities = new List<City>
            {
                new City {OriginalCityNumber = 0, X = 0, Y = 0},
                new City {OriginalCityNumber = 1, X = 2, Y = 0},
                new City {OriginalCityNumber = 2, X = 2, Y = 2},
                new City {OriginalCityNumber = 3, X = 0, Y = 2},
            };
        }

        [Test]
        public void ErrorForFourCitiesIsSame()
        {
            bnb = new BranchAndBound(fourCities);
            var expectedError = Math.Abs(0 - 2) + Math.Abs(0 - 0) + Math.Abs(2 - 0) + Math.Abs(0 - 2) +
                                Math.Abs(0 - 2) + Math.Abs(2 - 2) + Math.Abs(2 - 0) + Math.Abs(2 - 0);
            var actualError = bnb.CalculateError(fourCities);

            Assert.AreEqual(expectedError, actualError);
        }

        [Test]
        public void RequestingOrderedCitiesAfterARunAsyncResultsInCorrectOrder()
        {
            bnb = new BranchAndBound(fourCities);
            _ = bnb.RunAsync();
            var expected = orderedCities;
            var actual = bnb.GetBestScenarioFound();

            Assert.AreEqual(expected[0], actual[0]);
        }
    }
}