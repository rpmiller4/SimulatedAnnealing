using System.Collections.Generic;

namespace SimulatedAnnealing.Models
{
    public class PathAndCost
    {
        public List<City> Path { get; set; }
        public float? Cost { get; set; }
    }
}
