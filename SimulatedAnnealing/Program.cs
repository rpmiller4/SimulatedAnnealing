using System;

namespace SimulatedAnnealing
{
    class Program
    {
        static void Main(string[] args)
        {
            IrisDataCluster classifier = new IrisDataCluster(3);
            classifier.Run();

            //TravellingSalespersonProblem tsp = new TravellingSalespersonProblem(50);
            //tsp.Run();

            //TravellingSalespersonProblem tsp = new TravellingSalespersonProblem("./data/cities.data");
            //tsp.Run();
            //for (int i = 0; i < 3; i++)
            //{
            //    tsp.RunTwoOpt();
            //    tsp.RunTwoOpt();
            //    tsp.RunTwoOpt();
            //}



        }
    }
}
