using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SimulatedAnnealing.Core;
using SimulatedAnnealing.Models;
using SimulatedAnnealing.Utilities;

namespace SimulatedAnnealing.Problems
{
    public class TravelingSalespersonProblem : SimulatedAnnealingBase
    {
        private int nCitiesToGenerate;
        private List<City> citiesInOrder;
        private List<City> bestScenarioFound;

        private int lastFirstCityMutationIndex;
        private int lastSecondCityMutationIndex;

        private bool fromData = false;
        private string xyDataFile;
        private int inversionsSuccessful = 0;

        public TravelingSalespersonProblem(int citiesToGenerate)
        {
            nCitiesToGenerate = citiesToGenerate;
            fromData = false;
        }

        public TravelingSalespersonProblem(string xyDataFile)
        {
            fromData = true;
            this.xyDataFile = xyDataFile;
        }

        public List<City> GetCities()
        {
            return bestScenarioFound;
        }

        public override void LoadData()
        {
            if (fromData)
            {
                LoadDataFromCitiesFile();
            }
            else
            {
                GenerateData();
            }
        }

        private void LoadDataFromCitiesFile()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = " ",
                NewLine = Environment.NewLine,
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(xyDataFile))
            using (var csv = new CsvReader(reader, config))
            {
                citiesInOrder = csv.GetRecords<City>().ToList();
            }

            for (int i = 0; i < citiesInOrder.Count; i++)
            {
                citiesInOrder[i].Id = i;
            }
        }

        private void GenerateData()
        {
            List<City> cities = new List<City>();
            for (int i = 0; i < nCitiesToGenerate; i++)
            {
                cities.Add(new City
                {
                    Id = i,
                    X = GetRandomNumber(-100, 100),
                    Y = GetRandomNumber(-100, 100),
                    Z = GetRandomNumber(0, 20)
                });
            }
            this.citiesInOrder = cities;
        }

        public override void Mutate()
        {
            SwapCities();
        }

        private void SwapCities()
        {
            int firstCity = seed.Next(citiesInOrder.Count);
            int secondCity = (firstCity + seed.Next(citiesInOrder.Count)) % citiesInOrder.Count;

            City toSwap = citiesInOrder[firstCity];
            citiesInOrder[firstCity] = citiesInOrder[secondCity];
            citiesInOrder[secondCity] = toSwap;

            lastFirstCityMutationIndex = firstCity;
            lastSecondCityMutationIndex = secondCity;
        }

        public override void RevertLastMutation()
        {
            City toSwap = citiesInOrder[lastFirstCityMutationIndex];
            citiesInOrder[lastFirstCityMutationIndex] = citiesInOrder[lastSecondCityMutationIndex];
            citiesInOrder[lastSecondCityMutationIndex] = toSwap;
        }

        public override float CalculateError()
        {
            float error = 0;

            for (int i = 0; i < citiesInOrder.Count - 1; i++)
            {
                error += DistanceCalculator.CalculateManhattanDistance(citiesInOrder[i], citiesInOrder[i + 1]);
            }
            error += DistanceCalculator.CalculateManhattanDistance(citiesInOrder[^1], citiesInOrder[0]);
            return error;
        }

        private float CalculateError(List<City> route)
        {
            float error = 0;

            for (int i = 0; i < route.Count - 1; i++)
            {
                error += DistanceCalculator.CalculateManhattanDistance(route[i], route[i + 1]);
            }
            error += DistanceCalculator.CalculateManhattanDistance(route[^1], route[0]);
            return error;
        }

        protected override void OnBestSolutionFound(float error)
        {
            bestScenarioFound = new List<City>();
            for (int c = 0; c < citiesInOrder.Count; c++)
            {
                bestScenarioFound.Add(citiesInOrder[c].Copy());
            }
        }

        protected override void DisplayResults()
        {
            Console.WriteLine("List of cities in their new order.");

            for (int i = 0; i < citiesInOrder.Count; i++)
            {
                Console.Write(bestScenarioFound[i].Id + ", ");
            }
            Console.WriteLine();

            Console.WriteLine($"Best error {bestErrorFound}");
            Console.WriteLine($"Inversions Successful: {inversionsSuccessful}");
            inversionsSuccessful = 0;
        }

        /// <summary>
        /// Applies 2-opt optimization to improve the solution
        /// </summary>
        public void RunTwoOpt()
        {
            ApplyTwoOptInversion();
            DisplayResults();
        }

        private void ApplyTwoOptInversion()
        {
            int nodesToSwap = bestScenarioFound.Count - 1;
            for (int i = 0; i < nodesToSwap; i++)
            {
                for (int k = i + 1; k <= nodesToSwap; k++)
                {
                    var newRoute = TwoOptInversion(bestScenarioFound, i, k);
                    var newError = CalculateError(newRoute);
                    if (newError < bestErrorFound)
                    {
                        inversionsSuccessful++;
                        bestScenarioFound.Clear();
                        for (int c = 0; c < newRoute.Count; c++)
                        {
                            bestScenarioFound.Add(newRoute[c].Copy());
                        }
                        bestErrorFound = newError;
                    }
                }
            }
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/2-opt
        /// </summary>
        private List<City> TwoOptInversion(List<City> route, int i, int k)
        {
            List<City> newRoute = new List<City>();
            for (int a = 0; a < i; a++)
            {
                newRoute.Add(route[a].Copy());
            }
            for (int a = k; a >= i; a--)
            {
                newRoute.Add(route[a].Copy());
            }
            for (int a = k + 1; a < route.Count; a++)
            {
                newRoute.Add(route[a].Copy());
            }
            return newRoute;
        }
    }
}
