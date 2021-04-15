using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using SimulatedAnnealing.Utilities;

namespace SimulatedAnnealing
{
    public class VehicleRoutingProblem
    {
        private List<City> citiesInOrder;
        private List<TimeWindow> timeWindows;
        private Random seed = new Random();
        private int lastFirstCityMutationIndex;
        private int lastSecondCityMutationIndex;
        private int inversionsSuccessful;
        private float bestErrorFound;
        private List<City> bestScenarioFound;
        private int[][] timeDistances;

        public VehicleRoutingProblem()
        {
            LoadLocations();
            LoadTimeWindows();
            LoadTimeDistances();
        }

        public void Run()
        {
            float oldError = CalculateError();
            float error = float.PositiveInfinity;
            bestErrorFound = float.PositiveInfinity;
            bestScenarioFound = new List<City>();
            int epochs = 200000;
            float temperature = 2f;
            float coolingFactor = .999991f;

            for (int i = 0; i < epochs; i++)
            {
                temperature *= coolingFactor;
                Mutate();
                error = CalculateError();
                if (AcceptanceProbability(oldError, error, temperature) > (float)seed.NextDouble()) // keep solution
                {
                    oldError = error;
                }
                //else if (error < oldError) // keep solution
                //{
                //    oldError = error;
                //}
                else
                {
                    RevertLastMutation();
                }

                if (error < bestErrorFound)
                {
                    bestScenarioFound.Clear();
                    for (int c = 0; c < citiesInOrder.Count; c++)
                    {
                        bestScenarioFound.Add(citiesInOrder[c].Copy());
                    }
                    bestErrorFound = error;
                }

                Console.WriteLine($"old error: {oldError} error: {error} temperature: {temperature}");

            }
            DisplayResults();

        }
        public void LoadLocations()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = " ",
                NewLine = Environment.NewLine,
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader("./synthdata/locations.data"))
            using (var csv = new CsvReader(reader, config))
            {
                citiesInOrder = csv.GetRecords<City>().ToList();
            }
        }

        public void LoadTimeWindows()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = " ",
                NewLine = Environment.NewLine,
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader("./synthdata/timeWindows.data"))
            using (var csv = new CsvReader(reader, config))
            {
                timeWindows = csv.GetRecords<TimeWindow>().ToList();
            }
        }

        public void LoadTimeDistances()
        {
            timeDistances = new MatrixHelper().GetRecords("./synthdata/timeDistances.data");
        }

        public float AcceptanceProbability(float oldError, float newError, float temperature)
        {
            {
                return (float)Math.Exp((oldError - newError) / temperature);
            }
        }
        public float CalculateError()
        {
            float error = 0;

            for (int i = 0; i < citiesInOrder.Count - 1; i++)
            {
                error += CalculateDistance(citiesInOrder[i], citiesInOrder[i + 1]);
            }
            error += CalculateDistance(citiesInOrder[^1], citiesInOrder[0]);
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

        public void PrintAllDistances()
        {
            for (int i = 0; i < citiesInOrder.Count; i++)
            {
                for (int j = 0; j < citiesInOrder.Count; j++)
                {
                    Console.Write($"{CalculateDistance(citiesInOrder[i], citiesInOrder[j])} ");
                }
                Console.WriteLine();
            }
        }

        public void DisplayResults()
        {
            Console.WriteLine("List of cities in their new order.");

            for (int i = 0; i < citiesInOrder.Count; i++)
            {
                Console.Write(bestScenarioFound[i].Id + ", ");
            }

            Console.WriteLine($"Best error {bestErrorFound}");
            Console.WriteLine($"Inversions Successful: {inversionsSuccessful}");
            inversionsSuccessful = 0;
            DisplayTotalTimeCost();
        }

        public void DisplayTotalTimeCost()
        {
            int totalTime = 0;
            for (int i = 0; i < citiesInOrder.Count - 1; i++)
            {
                totalTime += timeDistances[bestScenarioFound[i].Id][bestScenarioFound[i + 1].Id];
            }
            Console.WriteLine($"totalTimeCost={totalTime}");
        }

        public void Mutate()
        {
            SwapCities();
        }

        public void RevertLastMutation()
        {
            City toSwap = citiesInOrder[lastFirstCityMutationIndex];
            citiesInOrder[lastFirstCityMutationIndex] = citiesInOrder[lastSecondCityMutationIndex];
            citiesInOrder[lastSecondCityMutationIndex] = toSwap;
        }

        public void SwapCities()
        {
            int firstCity = seed.Next(citiesInOrder.Count);
            int secondCity = (firstCity + seed.Next(citiesInOrder.Count)) % citiesInOrder.Count;

            City toSwap = citiesInOrder[firstCity];
            citiesInOrder[firstCity] = citiesInOrder[secondCity];
            citiesInOrder[secondCity] = toSwap;

            lastFirstCityMutationIndex = firstCity;
            lastSecondCityMutationIndex = secondCity;
        }
    }

    // When deliveries can be routed successfully
    public class TimeWindow
    {
        public int CityId { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
