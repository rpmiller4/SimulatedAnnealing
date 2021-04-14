using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatedAnnealing
{
    // See https://tspvis.com/ for BranchAndBound implementation in javascript
    public class BranchAndBound : IRun
    {
        //private PathAndCost currentPathAndCost;
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
            else
            {
                // stored already.
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
                citiesInOrder[i].OriginalCityNumber = i;
            }

            for (int i = 0; i < citiesInOrder.Count; i++)
            {
                SwapCities();
            }
        }

        public void SwapCities()
        {
            int firstCity = seed.Next(citiesInOrder.Count);
            int secondCity = (firstCity + seed.Next(citiesInOrder.Count)) % citiesInOrder.Count;

            City toSwap = citiesInOrder[firstCity];
            citiesInOrder[firstCity] = citiesInOrder[secondCity];
            citiesInOrder[secondCity] = toSwap;
        }

        public async Task RunAsync()
        {
            Load();
            PathAndCost pnc = await DoBranchAndBound(citiesInOrder);
            bestScenarioFound = pnc.Path;
        }

        /// <returns>Best route</returns>
        async public Task<PathAndCost> DoBranchAndBound(
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
                //Console.WriteLine("Bounding here because this is worst than the best solution");
                return new PathAndCost { Path = null, Cost = null };
            }

            if (available.Count == 0)
            {
                //bestScenarioFound = route;
                for (int i = 0; i < route.Count; i++)
                {
                    Console.Write(route[i].OriginalCityNumber + ", ");
                }
                return new PathAndCost { Path = route, Cost = error };
            }

            float? bestCost = float.PositiveInfinity;
            List<City> bestPath = null;

            foreach (var c in available)
            {
                //var copy = c.Copy();

                visited.Add(c);
                route.Add(c);

                PathAndCost currentPathAndCost = await DoBranchAndBound(points, route, visited, overallBest);

                if (currentPathAndCost.Cost < bestCost)
                {
                    //Console.WriteLine("Found an improvement.");
                    bestCost = currentPathAndCost.Cost;
                    bestPath = currentPathAndCost.Path;
                    
                    if (bestCost < overallBest)
                    {
                        Console.WriteLine($"best Error = {bestCost}, overall best = {overallBest}");
                        overallBest = bestCost;
                        //Console.WriteLine($"cities in route: {currentPathAndCost.Path.Count}");
                    }
                    
                }
                //Console.WriteLine($"Removing inserted indexes {visitedInsertionIndex} and {routeInsertionIndex}");
                visited.Remove(c);
                route.Remove(c);
            }
            return new PathAndCost { Cost = bestCost, Path = bestPath };
        }


        public float CalculateError(List<City> route)
        {
            //Console.WriteLine("calculating error");
            float error = 0;

            for (int i = 0; i < route.Count - 1; i++)
            {
                error += CalculateDistance(route[i], route[i + 1]);
            }
            error += CalculateDistance(route[^1], route[0]);
            return error;
        }

        //Euclidean
        private float CalculateDistance(City firstCity, City secondCity)
        {
            float distance = 0;
            distance += Math.Abs(firstCity.X - secondCity.X);
            distance += Math.Abs(firstCity.Y - secondCity.Y);
            return distance;
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public List<City> GetBestScenarioFound()
        {
            return bestScenarioFound;
        }

        public class PathAndCost
        {
            public List<City> Path { get; set; }
            public float? Cost { get; set; }
        }
    }
}
