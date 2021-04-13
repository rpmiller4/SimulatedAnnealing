using System;

namespace SimulatedAnnealing
{
    class Program
    {
        static void Main(string[] args)
        {
            IrisDataCluster classifier = new IrisDataCluster(3);
            classifier.Run();
        }
    }
}
