using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SimulatedAnnealing
{
    class TravellingSalespersonProblem : IAnneal
    {
        private Random seed = new Random();
        int nCitiesToGenerate;
        private List<City> citiesInOrder;
        private List<City> bestScenarioFound;
        private float bestErrorFound;

        int lastFirstCityMutationIndex;
        int lastSecondCityMutationIndex;
        City smootherCity;

        bool fromData = false;
        string xyDataFile;

        public TravellingSalespersonProblem(int citiesToGenerate)
        {
            nCitiesToGenerate = citiesToGenerate;
            fromData = false;
        }

        public TravellingSalespersonProblem(string XyDataFile)
        {
            fromData = true;
            xyDataFile = XyDataFile;
        }

        public void LoadData()
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
                citiesInOrder[i].OriginalCityNumber = i;
            }
        }

        private void GenerateData()
        {
            
            List<City> cities = new List<City>();
            for (int i = 0; i < nCitiesToGenerate; i++)
            {
                cities.Add(new City
                {
                    OriginalCityNumber = i,
                    X = GetRandomNumber(-100, 100),
                    Y = GetRandomNumber(-100, 100),
                    Z = GetRandomNumber(0, 20)
                });
            }
            this.citiesInOrder = cities;
        }

        public void Run()
        {
            LoadData();

            float oldError = CalculateError();
            float error = float.PositiveInfinity;
            bestErrorFound = float.PositiveInfinity;
            bestScenarioFound = new List<City>();
            int epochs = 10000000;
            float temperature = 1f;
            float coolingFactor = .999999f;

            for (int i = 0; i < epochs; i++)
            {
                temperature *= coolingFactor;
                Mutate();
                error = CalculateError();
                if (temperature > (float)seed.NextDouble()) // keep solution
                {
                    oldError = error;
                }
                else if (error < oldError) // keep solution
                {
                    oldError = error;
                }
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

        public void RunTwoOpt()
        {
            ApplyTwoOptInversion();
            DisplayResults();
        }

        public void Mutate()
        {
            SwapCities();
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

        int inversionsSuccessful = 0;

        public void ApplyTwoOptInversion()
        {
            int nodesToSwap = bestScenarioFound.Count - 1;
            for (int i = 0; i < nodesToSwap; i++)
            {
                for(int k = i + 1; k <= nodesToSwap; k++)
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
        public List<City> TwoOptInversion(List<City> route, int i, int k)
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
            for (int a = k+1; a < route.Count; a++)
            {
                newRoute.Add(route[a].Copy());
            }
            return newRoute;
        }

        public void RevertLastMutation()
        {
            City toSwap = citiesInOrder[lastFirstCityMutationIndex];
            citiesInOrder[lastFirstCityMutationIndex] = citiesInOrder[lastSecondCityMutationIndex];
            citiesInOrder[lastSecondCityMutationIndex] = toSwap;
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


        public float CalculateError(List<City> route)
        {
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

        public void DisplayResults()
        {
            Console.WriteLine("List of cities in their new order.");

            for (int i = 0; i < citiesInOrder.Count; i++)
            {
                Console.Write(bestScenarioFound[i].OriginalCityNumber + ", ");
            }

            Console.WriteLine($"Best error {bestErrorFound}");
            Console.WriteLine($"Inversions Successful: {inversionsSuccessful}");
            inversionsSuccessful = 0;
        }

        public float GetRandomNumber(float minimum, float maximum)
        {
            return (float)seed.NextDouble() * (maximum - minimum) + minimum;
        }
    }

    public class City
    {
        public int OriginalCityNumber { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public City Copy()
        {
            return new City
            {
                OriginalCityNumber = this.OriginalCityNumber,
                X = X,
                Y = Y,
                Z = Z
            };
        }

    }
}
