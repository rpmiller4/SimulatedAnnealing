using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SimulatedAnnealing.VRP;

namespace SimulatedAnnealing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //IrisDataCluster classifier = new IrisDataCluster(3);
            //classifier.RunAsync();

            //TravelingSalespersonProblem tsp = new TravelingSalespersonProblem(50);
            //tsp.Run();

            //TravelingSalespersonProblem tsp = new TravelingSalespersonProblem("./data/cities.data");
            //tsp.Run();
            //for (int i = 0; i < 3; i++)
            //{
            //    //tsp.RunRestart();
            //    tsp.RunTwoOpt();
            //    tsp.RunTwoOpt();
            //    tsp.RunTwoOpt();
            //}

            //TravellingSalespersonProblem tsp = new TravellingSalespersonProblem("./data/cities.data");
            //tsp.Run();
            //BranchAndBound bnb = new BranchAndBound(tsp.GetCities());
            //await bnb.RunAsync();

            //BranchAndBound bnb = new BranchAndBound("./data/cities.data");
            //await bnb.RunAsync();

            VehicleRoutingProblem vrp = new VehicleRoutingProblem();
            vrp.Run();
        }
    }
}
