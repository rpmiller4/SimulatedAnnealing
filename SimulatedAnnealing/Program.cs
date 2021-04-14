using System;
using System.Threading.Tasks;

namespace SimulatedAnnealing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //IrisDataCluster classifier = new IrisDataCluster(3);
            //classifier.RunAsync();

            //TravellingSalespersonProblem tsp = new TravellingSalespersonProblem(50);
            //tsp.Run();

            TravellingSalespersonProblem tsp = new TravellingSalespersonProblem("./data/cities.data");
            tsp.Run();
            for (int i = 0; i < 3; i++)
            {
                //tsp.RunRestart();
                tsp.RunTwoOpt();
                tsp.RunTwoOpt();
                tsp.RunTwoOpt();
            }

            //TravellingSalespersonProblem tsp = new TravellingSalespersonProblem("./data/cities.data");
            //tsp.Run();
            //BranchAndBound bnb = new BranchAndBound(tsp.GetCities());
            //await bnb.RunAsync();

            //BranchAndBound bnb = new BranchAndBound("./data/cities.data");
            //await bnb.RunAsync();

        }
    }
}
