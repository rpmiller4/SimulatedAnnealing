using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using SimulatedAnnealing.Core;
using SimulatedAnnealing.Models;
using SimulatedAnnealing.Utilities;

namespace SimulatedAnnealing.Problems
{
    public class VehicleRoutingProblem : SimulatedAnnealingBase
    {
        private List<City> citiesInOrder;
        private List<TimeWindow> timeWindows;
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
            nVehicles = vehicles;

            // Override default parameters for VRP
            Epochs = 1400000;
            InitialTemperature = 1000f;
            CoolingFactor = 0.999992f;
        }

        public override void LoadData()
        {
            LoadLocations();
            LoadTimeWindows();
            LoadTimeDistances();
            BuildRoutes();
        }

        private void LoadLocations()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = " ",
                NewLine = Environment.NewLine,
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(DataConfiguration.LocationsDataFile))
            using (var csv = new CsvReader(reader, config))
            {
                citiesInOrder = csv.GetRecords<City>().ToList();
            }
        }

        private void LoadTimeWindows()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = " ",
                NewLine = Environment.NewLine,
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(DataConfiguration.TimeWindowsDataFile))
            using (var csv = new CsvReader(reader, config))
            {
                timeWindows = csv.GetRecords<TimeWindow>().ToList();
            }
        }

        private void LoadTimeDistances()
        {
            timeDistances = new MatrixHelper().GetRecords(DataConfiguration.TimeDistancesDataFile);
        }

        private Appointment CreateAppointment(City city)
        {
            var appointment = new Appointment { CityId = city.Id };
            var startTime = seed.Next(timeWindows[city.Id].Start, timeWindows[city.Id].End);
            appointment.Start = startTime;
            appointment.End = startTime + 1;
            appointment.VehicleId = seed.Next(nVehicles);

            return appointment;
        }

        private void BuildRoutes()
        {
            appointments = new List<Appointment>();
            foreach (var c in citiesInOrder)
            {
                appointments.Add(CreateAppointment(c));
            }
        }

        public override void Mutate()
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

        public override void RevertLastMutation()
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

        private void SwapVehicleAssignment(Appointment left, Appointment right)
        {
            var temporary = left.VehicleId;
            left.VehicleId = right.VehicleId;
            right.VehicleId = temporary;
        }

        private void SwapVehicleAssignment(Appointment appointment)
        {
            oldTimeBlock = appointment.Copy();
            appointment.VehicleId = (appointment.VehicleId + seed.Next(nVehicles)) % nVehicles;
        }

        private void MutateTimeBlock(Appointment appointment)
        {
            oldTimeBlock = appointment.Copy();

            switch (seed.Next(3))
            {
                case 0:
                    MutateTimeShift(appointment);
                    break;
                case 1:
                    MutateTimeRandom(appointment);
                    break;
                case 2:
                    MutateTimeShrink(appointment);
                    break;
            }
        }

        private void UndoMutateTimeBlock()
        {
            appointments[lastRouteMutated] = oldTimeBlock;
        }

        private void MutateTimeShift(Appointment appointment)
        {
            int shiftDirection = seed.Next(2) == 0 ? -1 : 1;
            appointment.Start = (appointment.Start + shiftDirection) % HoursInADay;
            appointment.End = (appointment.End + shiftDirection) % HoursInADay;
        }

        private void MutateTimeRandom(Appointment appointment)
        {
            appointment.Start = seed.Next(0, HoursInADay) % HoursInADay;
            appointment.End = seed.Next(0, HoursInADay) % HoursInADay;
        }

        private void MutateTimeShrink(Appointment appointment)
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

        public override float CalculateError()
        {
            float error = 0;

            routes = new List<List<City>>();

            for (int i = 0; i < nVehicles; i++)
            {
                routes.Add(new List<City>());
                var relevantAppointments = appointments.Where(x => x.VehicleId == i).OrderBy(x => x.Start).ToList();

                foreach (var ra in relevantAppointments)
                {
                    routes[i].Add(citiesInOrder.First(x => x.Id == ra.CityId));
                }
            }

            for (int i = 0; i < routes.Count; i++)
            {
                if (routes[i].Any())
                {
                    for (int j = 0; j < routes[i].Count - 1; j++)
                    {
                        error += DistanceCalculator.CalculateManhattanDistance(routes[i][j], routes[i][j + 1]);
                    }

                    error += DistanceCalculator.CalculateManhattanDistance(routes[i][0], citiesInOrder.First(x => x.Id == 0));
                    error += DistanceCalculator.CalculateManhattanDistance(citiesInOrder.First(x => x.Id == 0), routes[i][^1]);
                }
            }

            error += CalculateFairness();
            error += CalculateTimePrecision();
            error += CalculateTimeOverlap();

            return error;
        }

        private float CalculateFairness()
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
                for (int j = 0; j < relevantAppts.Count - 1; j++)
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

        protected override void OnBestSolutionFound(float error)
        {
            bestScenarioFound = new List<Appointment>();
            for (int k = 0; k < appointments.Count; k++)
            {
                bestScenarioFound.Add(appointments[k].Copy());
            }
        }

        protected override void DisplayResults()
        {
            Console.WriteLine("List of routes in their new order.");
            Console.WriteLine($"Best error found: {bestErrorFound}");
            DisplayTimeWindows();
        }

        private void DisplayTimeWindows()
        {
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
    }
}
