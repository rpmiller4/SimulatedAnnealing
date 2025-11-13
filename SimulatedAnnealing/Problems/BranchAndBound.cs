using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimulatedAnnealing.Core;
using SimulatedAnnealing.Models;
using SimulatedAnnealing.Utilities;

namespace SimulatedAnnealing.Problems
{
    /// <summary>
    /// Branch and Bound exact algorithm for solving TSP
    /// See https://tspvis.com/ for implementation reference
    /// </summary>
    public class BranchAndBound : IRun
    {
        private string xyDataFile;
        private List<City> citiesInOrder;
        private Random seed = new Random();
        private bool fromDataFile = false;
        private List<City> bestScenarioFound;

        public BranchAndBound(string dataFile)
        {
            xyDataFile = dataFile;
            fromDataFile = true;
        }

        public BranchAndBound(List<City> cities)
        {
            citiesInOrder = cities;
            fromDataFile = false;
        }

        public void Load()
        {
            if (fromDataFile)
            {
                LoadDataFromCitiesFile();
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

            // Shuffle cities to start with a random configuration
            for (int i = 0; i < citiesInOrder.Count; i++)
            {
                SwapCities();
            }
        }

        private void SwapCities()
        {
            int firstCity = seed.Next(citiesInOrder.Count);
            int secondCity = (firstCity + seed.Next(citiesInOrder.Count)) % citiesInOrder.Count;

            City toSwap = citiesInOrder[firstCity];
            citiesInOrder[firstCity] = citiesInOrder[secondCity];
            citiesInOrder[secondCity] = toSwap;
        }

        public void Run()
        {
            Load();
            var result = DoBranchAndBoundSync(citiesInOrder);
            bestScenarioFound = result.Path;
            Console.WriteLine($"Branch and Bound completed with cost: {result.Cost}");
        }

        public async Task RunAsync()
        {
            Load();
            PathAndCost pnc = await DoBranchAndBound(citiesInOrder);
            bestScenarioFound = pnc.Path;
            Console.WriteLine($"Branch and Bound completed with cost: {pnc.Cost}");
        }

        /// <summary>
        /// Synchronous version of Branch and Bound algorithm
        /// </summary>
        private PathAndCost DoBranchAndBoundSync(
            List<City> points,
            List<City> route = null,
            List<City> visited = null,
            float? overallBest = float.PositiveInfinity)
        {
            if (visited == null)
            {
                var firstPoint = points[0];
                points.Remove(firstPoint);
                route = new List<City>();
                route.Add(firstPoint);
                visited = new List<City>();
            }
            var available = points.Except(visited).ToList();

            var error = CalculateError(route);

            if (error > overallBest)
            {
                return new PathAndCost { Path = null, Cost = null };
            }

            if (available.Count == 0)
            {
                for (int i = 0; i < route.Count; i++)
                {
                    Console.Write(route[i].Id + ", ");
                }
                return new PathAndCost { Path = route, Cost = error };
            }

            float? bestCost = float.PositiveInfinity;
            List<City> bestPath = null;

            foreach (var c in available)
            {
                visited.Add(c);
                route.Add(c);

                PathAndCost currentPathAndCost = DoBranchAndBoundSync(points, route, visited, overallBest);

                if (currentPathAndCost.Cost < bestCost)
                {
                    bestCost = currentPathAndCost.Cost;
                    bestPath = currentPathAndCost.Path;

                    if (bestCost < overallBest)
                    {
                        Console.WriteLine($"best Error = {bestCost}, overall best = {overallBest}");
                        overallBest = bestCost;
                    }
                }

                visited.Remove(c);
                route.Remove(c);
            }
            return new PathAndCost { Cost = bestCost, Path = bestPath };
        }

        /// <summary>
        /// Asynchronous version of Branch and Bound algorithm
        /// </summary>
        private async Task<PathAndCost> DoBranchAndBound(
            List<City> points,
            List<City> route = null,
            List<City> visited = null,
            float? overallBest = float.PositiveInfinity)
        {
            if (visited == null)
            {
                var firstPoint = points[0];
                points.Remove(firstPoint);
                route = new List<City>();
                route.Add(firstPoint);
                visited = new List<City>();
            }
            var available = points.Except(visited).ToList();

            var error = CalculateError(route);

            if (error > overallBest)
            {
                return new PathAndCost { Path = null, Cost = null };
            }

            if (available.Count == 0)
            {
                for (int i = 0; i < route.Count; i++)
                {
                    Console.Write(route[i].Id + ", ");
                }
                return new PathAndCost { Path = route, Cost = error };
            }

            float? bestCost = float.PositiveInfinity;
            List<City> bestPath = null;

            foreach (var c in available)
            {
                visited.Add(c);
                route.Add(c);

                PathAndCost currentPathAndCost = await DoBranchAndBound(points, route, visited, overallBest);

                if (currentPathAndCost.Cost < bestCost)
                {
                    bestCost = currentPathAndCost.Cost;
                    bestPath = currentPathAndCost.Path;

                    if (bestCost < overallBest)
                    {
                        Console.WriteLine($"best Error = {bestCost}, overall best = {overallBest}");
                        overallBest = bestCost;
                    }
                }

                visited.Remove(c);
                route.Remove(c);
            }
            return new PathAndCost { Cost = bestCost, Path = bestPath };
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

        public List<City> GetBestScenarioFound()
        {
            return bestScenarioFound;
        }
    }
}
