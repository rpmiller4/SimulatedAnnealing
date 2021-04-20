using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Xml.Schema;
using CsvHelper;
using CsvHelper.Configuration;
using SimulatedAnnealing;
using SimulatedAnnealing.Utilities;

namespace SimulatedAnnealing.VRP
{
    public class VehicleRoutingProblem
    {
        private List<City> citiesInOrder;
        private List<TimeWindow> timeWindows;
        private Random seed = new Random();
        private float bestErrorFound;
        private List<Appointment> bestScenarioFound;
        private int[][] timeDistances;
        private int nVehicles;

        private List<Appointment> appointments;

        private int lastRouteMutated;


        private Appointment oldTimeBlock;
        private Appointment oldLeftAppointment;
        private Appointment oldRightAppointment;

        private const int HoursInADay = 24;

        private bool lastMutationWasOneAppt;
        private List<List<City>> routes;

        public VehicleRoutingProblem(int vehicles = 3)
        {
            LoadLocations();
            LoadTimeWindows();
            LoadTimeDistances();
            nVehicles = vehicles;
            BuildRoutes();
        }

        public void Run()
        {
            float oldError = CalculateError();
            float error = float.PositiveInfinity;
            bestErrorFound = float.PositiveInfinity;
            bestScenarioFound = new List<Appointment>();
            int epochs = 1400000;
            float temperature = 1000f;
            float coolingFactor = .999992f;

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
                    bestErrorFound = error;
                    bestScenarioFound.Clear();
                    for (int k = 0; k < appointments.Count; k++)
                    { 
                        bestScenarioFound.Add(appointments[k].Copy());
                    }
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

        public Appointment CreateAppointment(City city)
        {
            var appointment = new Appointment {CityId = city.Id};
            var startTime = seed.Next(timeWindows[city.Id].Start, timeWindows[city.Id].End);
            appointment.Start = startTime;
            appointment.End = startTime + 1;
            appointment.VehicleId = seed.Next(nVehicles);

            return appointment;
        }

        public void Mutate()
        {
            if (seed.Next(2) == 0)
            {
                var apptOneIndex = seed.Next(appointments.Count);
                var apptTwoIndex = seed.Next(appointments.Count);
                var appointmentOne = appointments[apptOneIndex];
                var appointmentTwo = appointments[apptTwoIndex];
                oldLeftAppointment = appointmentOne;
                oldRightAppointment = appointmentTwo;
                lastMutationWasOneAppt = false;
                if (seed.Next(2) == 0)
                {
                    SwapVehicleAssignment(appointmentOne, appointmentTwo);
                }
                else
                {
                    lastMutationWasOneAppt = true;
                    lastRouteMutated = seed.Next(appointments.Count);
                    SwapVehicleAssignment(appointments[lastRouteMutated]);
                }
            }
            else
            {
                lastMutationWasOneAppt = true;
                lastRouteMutated = seed.Next(appointments.Count);
                MutateTimeBlock(appointments[lastRouteMutated]);
            }
        }

        public void RevertLastMutation()
        {
            if (lastMutationWasOneAppt)
            {
                UndoMutateTimeBlock();
            }
            else
            {
                SwapVehicleAssignment(oldLeftAppointment, oldRightAppointment);
            }
        }

        public void SwapVehicleAssignment(Appointment left, Appointment right)
        {
            var temporary = left.VehicleId;
            left.VehicleId = right.VehicleId;
            right.VehicleId = temporary;
        }

        public void SwapVehicleAssignment(Appointment appointment)
        {
            oldTimeBlock = appointment.Copy();
            appointment.VehicleId = (appointment.VehicleId + seed.Next(nVehicles)) % nVehicles;
        }

        public void MutateTimeBlock(Appointment appointment)
        {
            oldTimeBlock = appointment.Copy();

            switch (seed.Next(3))
            {
                case 0:
                {
                    //shift left or right
                    MutateTimeShift(appointment);
                    break;
                }
                case 1:
                {
                    //random mutation
                    MutateTimeRandom(appointment);
                    break;
                }
                case 2:
                {
                    //shrink normalize?
                    MutateTimeShrink(appointment);
                    break;
                }
            }
        }

        public void UndoMutateTimeBlock()
        {
            appointments[lastRouteMutated] = oldTimeBlock;
        }


        public void MutateTimeShift(Appointment appointment)
        {
            int shiftDirection = seed.Next(2) == 0 ? -1 : 1;
            appointment.Start = (appointment.Start + shiftDirection) % HoursInADay;
            appointment.End = (appointment.End + shiftDirection) % HoursInADay;
        }


        public void MutateTimeRandom(Appointment appointment)
        {
            appointment.Start = seed.Next(0, HoursInADay) % HoursInADay;
            appointment.End = seed.Next(0, HoursInADay) % HoursInADay;
        }

        public void MutateTimeShrink(Appointment appointment)
        {
            bool shrinkLeftToRight = seed.Next(2) == 0;

            if (shrinkLeftToRight)
            {
                appointment.Start = (appointment.Start + 1) % HoursInADay;
            }
            else
            {
                appointment.End = (appointment.End - 1) % HoursInADay;
            }

        }

        public void BuildRoutes()
        {
            appointments = new List<Appointment>();
            foreach (var c in citiesInOrder)
            {
                appointments.Add(CreateAppointment(c));
            }
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

            routes = new List<List<City>>();

            for (int i = 0; i < nVehicles; i++) // time complexity suspect here.
            {
                routes.Add(new List<City>());
                var relevantAppointments = appointments.Where(x => x.VehicleId == i).OrderBy(x => x.Start).ToList();

                foreach (var ra in relevantAppointments)
                {
                    routes[i].Add(citiesInOrder.First(x=>x.Id == ra.CityId));
                }
                
            }

            for (int i = 0; i < routes.Count; i++)
            {
                if (routes[i].Any())
                {
                    for (int j = 0; j < routes[i].Count - 1; j++)
                    {
                        error += CalculateDistance(routes[i][j], routes[i][j + 1]);
                    }

                    error += CalculateDistance(routes[i][0], citiesInOrder.First(x => x.Id == 0));
                    error += CalculateDistance(citiesInOrder.First(x => x.Id == 0), routes[i][^1]);
                }
            }

            error += CalculateFairness();
            error += CalculateTimePrecision();
            error += CalculateTimeOverlap();


            return error;
        }



        private float CalculateFairness() // assume drivers want equal n routes.
        {
            float mse = 0;
            var average = citiesInOrder.Count / routes.Count;

            for (int i = 0; i < routes.Count; i++)
            {
                mse += (float)Math.Pow(routes[i].Count - average, 2);
            }

            return mse;
        }


        private float CalculateTimePrecision()
        {
            float error = 0;
            foreach (var a in appointments)
            {
                var respectiveCityTimeWindow = timeWindows.First(x => x.CityId == a.CityId);
                if (a.Start > a.End)
                {
                    error += a.Start - a.End;
                }

                if (a.Start == a.End)
                {
                    error += 2;
                }

                if (a.Start < respectiveCityTimeWindow.Start)
                {
                    error += respectiveCityTimeWindow.Start - a.Start;
                }

                if (a.End > respectiveCityTimeWindow.End)
                {
                    error += a.End - respectiveCityTimeWindow.End;
                }
            }

            return error;
        }
        private float CalculateTimeOverlap()
        {

            float error = 0;
            for (int i = 0; i < nVehicles; i++)
            {
                var relevantAppts = appointments.Where(x => x.VehicleId == i).OrderBy(x => x.Start).ToList();
                for (int j = 0; j < relevantAppts.Count -1; j++)
                {

                    if (relevantAppts[j].Start > relevantAppts[j + 1].Start)
                    {
                        error += relevantAppts[j].Start - relevantAppts[j + 1].Start;
                    }
                    if (relevantAppts[j].End > relevantAppts[j + 1].End)
                    {
                        error += relevantAppts[j].End - relevantAppts[j + 1].End;
                    }
                    if (relevantAppts[j].Start == relevantAppts[j + 1].Start)
                    {
                        error += 5;
                    }
                    if (relevantAppts[j].End == relevantAppts[j + 1].End)
                    {
                        error += 5;
                    }
                }
            }

            return error;
        }

        private void DisplayTimeWindows()
        {
            
            float error = 0;
            for (int i = 0; i < nVehicles; i++)
            {
                var relevantAppts = appointments.Where(x => x.VehicleId == i).OrderBy(x => x.Start).ToList();
                Console.WriteLine($"For vehicle {i}");
                foreach (var ra in relevantAppts)
                {
                    var correspondingTimeWindow = timeWindows.First(x => x.CityId == ra.CityId);
                    Console.WriteLine($"At {ra.CityId}: {ra.Start}-{ra.End} vs {correspondingTimeWindow.Start}-{correspondingTimeWindow.End}");
                }
            }
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
            Console.WriteLine("List of routes in their new order.");

            DisplayRoutes();
            DisplayTimeWindows();
        }


        public void DisplayRoutes()
        {
            //for (int i = 0; i < bestScenarioFound.Count; i++)
            //{
            //    Console.WriteLine($"Routes for vehicle {i}:");

            //    Console.Write($"{bestScenarioFound[i].CityId}, ");
                
            //    Console.WriteLine();
            //}

        }


    }

    // When deliveries can be routed successfully
    public class TimeWindow
    {
        public int CityId { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }

    public class VehicleSchedule
    {
        public List<Appointment> ScheduleBlocks { get; set; }
    }

    public class Appointment
    {
        public int CityId { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int VehicleId { get; set; }

        public Appointment Copy()
        {
            return new Appointment()
            {
                CityId = CityId,
                Start = Start,
                End = End,
                VehicleId = VehicleId
            };
        }
    }
}
