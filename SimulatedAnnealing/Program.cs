using System;
using System.Threading.Tasks;
using SimulatedAnnealing.Core;
using SimulatedAnnealing.Problems;

namespace SimulatedAnnealing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Simulated Annealing Library ===");
            Console.WriteLine();

            // Example 1: Iris Data Clustering
            // IrisDataCluster classifier = new IrisDataCluster(3);
            // await classifier.RunAsync();

            // Example 2: TSP with generated cities
            // TravelingSalespersonProblem tsp = new TravelingSalespersonProblem(50);
            // tsp.Run();

            // Example 3: TSP from data file
            // TravelingSalespersonProblem tsp = new TravelingSalespersonProblem(DataConfiguration.CitiesDataFile);
            // tsp.Run();
            // Apply 2-opt optimization
            // for (int i = 0; i < 3; i++)
            // {
            //     tsp.RunTwoOpt();
            // }

            // Example 4: Branch and Bound exact algorithm
            // TravelingSalespersonProblem tsp = new TravelingSalespersonProblem(DataConfiguration.CitiesDataFile);
            // tsp.Run();
            // BranchAndBound bnb = new BranchAndBound(tsp.GetCities());
            // await bnb.RunAsync();

            // Example 5: Branch and Bound from data file
            // BranchAndBound bnb = new BranchAndBound(DataConfiguration.CitiesDataFile);
            // await bnb.RunAsync();

            // Example 6: Vehicle Routing Problem with Time Windows
            VehicleRoutingProblem vrp = new VehicleRoutingProblem();
            vrp.Run();

            Console.WriteLine();
            Console.WriteLine("=== Execution Complete ===");
        }
    }
}
