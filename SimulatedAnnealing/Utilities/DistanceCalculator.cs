using System;
using SimulatedAnnealing.Models;

namespace SimulatedAnnealing.Utilities
{
    public static class DistanceCalculator
    {
        /// <summary>
        /// Calculates Manhattan distance between two cities (L1 norm)
        /// </summary>
        public static float CalculateManhattanDistance(City firstCity, City secondCity)
        {
            float distance = 0;
            distance += Math.Abs(firstCity.X - secondCity.X);
            distance += Math.Abs(firstCity.Y - secondCity.Y);
            return distance;
        }

        /// <summary>
        /// Calculates Euclidean distance between two cities (L2 norm)
        /// </summary>
        public static float CalculateEuclideanDistance(City firstCity, City secondCity)
        {
            float dx = firstCity.X - secondCity.X;
            float dy = firstCity.Y - secondCity.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Calculates squared Euclidean distance between two points (avoids sqrt for performance)
        /// </summary>
        public static float CalculateSquaredDistance(float x1, float y1, float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            return dx * dx + dy * dy;
        }
    }
}
