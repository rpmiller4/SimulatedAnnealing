using System.Threading.Tasks;

namespace SimulatedAnnealing
{
    public interface IRun
    {
        Task RunAsync();
        void Run();
    }
}