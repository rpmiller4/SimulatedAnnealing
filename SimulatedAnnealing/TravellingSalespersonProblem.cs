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
        int smootherCityIndex;
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

            //create a fake city to smooth out search space?
            //citiesInOrder.Add(new City
            //{
            //    OriginalCityNumber = -1,
            //    X = GetRandomNumber(-100, 100),
            //    Y = GetRandomNumber(-100, 100),
            //    //Z = GetRandomNumber(0, 20)
            //});
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


        private float CoolingSchedule(float temperature)
        {
            if (temperature > .5)
            {
                temperature *= .99999f;
            }
            else if (temperature > .015)
            {
                temperature *= .999999f;
            }
            else if (temperature > .0001)
            {
                temperature *= .9999995f;
            }
            else temperature *= .9999999f;
            return temperature;
        }
        public void Run()
        {
            LoadData();

            float oldError = CalculateError();
            float error = float.PositiveInfinity;
            bestErrorFound = float.PositiveInfinity;
            bestScenarioFound = new List<City>();
            int epochs = 100000;
            float temperature = .6f;
            float coolingFactor = .995f;

            for (int i = 0; i < epochs; i++)
            {
                //temperature -= coolingFactor;
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
                        bestScenarioFound.Add(citiesInOrder[c].Copy()); // grr redundancy.
                    }
                    bestErrorFound = error;
                }

                Console.WriteLine($"old error: {oldError} error: {error} temperature: {temperature}");

            }
            DisplayResults();

        }

        public void RunOptimizations()
        {
            citiesInOrder.Clear();
            for (int c = 0; c < bestScenarioFound.Count; c++)
            {
                citiesInOrder.Add(bestScenarioFound[c].Copy()); // grr redundancy.
            }

            float oldError = bestErrorFound;
            float error = bestErrorFound;

            int epochs = 150000;
            float temperature = .0001f;
            float coolingFactor = .99997f;

            for (int i = 0; i < epochs; i++)
            {
                //temperature -= coolingFactor;
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
                        bestScenarioFound.Add(citiesInOrder[c].Copy()); // grr redundancy.
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
            //MutateSmootherCity();
        }

        public void SwapCities()
        {
            int firstCity = seed.Next(citiesInOrder.Count);
            int secondCity = (firstCity + seed.Next(citiesInOrder.Count-1)) % citiesInOrder.Count;

            City toSwap = citiesInOrder[firstCity];
            citiesInOrder[firstCity] = citiesInOrder[secondCity];
            citiesInOrder[secondCity] = toSwap;

            lastFirstCityMutationIndex = firstCity;
            lastSecondCityMutationIndex = secondCity;
        }

        public void MutateSmootherCity()
        {
             
            City toMutate = citiesInOrder.Where(x => x.OriginalCityNumber == -1).First();
            smootherCity = toMutate.Copy();

            toMutate.X += GetRandomNumber(-5000f, 5000f);
            toMutate.Y += GetRandomNumber(-5000f, 5000f);
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

            //City toMutate = citiesInOrder.Where(x => x.OriginalCityNumber == -1).First();
            //toMutate.X = smootherCity.X;
            //toMutate.Y = smootherCity.Y;
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
            //distance += Math.Abs(firstCity.Z - secondCity.Z);
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
                X = this.X,
                Y = this.Y,
                Z = this.Z
            };
        }

    }
}
