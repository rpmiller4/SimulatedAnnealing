using System;
using System.Threading.Tasks;

namespace SimulatedAnnealing.Core
{
    /// <summary>
    /// Base class for simulated annealing optimization algorithms.
    /// Provides the core SA algorithm while allowing subclasses to define problem-specific logic.
    /// </summary>
    public abstract class SimulatedAnnealingBase : IAnneal
    {
        protected Random seed = new Random();
        protected float bestErrorFound;

        /// <summary>
        /// Gets or sets the number of iterations for the annealing process
        /// </summary>
        protected virtual int Epochs { get; set; } = 400000;

        /// <summary>
        /// Gets or sets the initial temperature for the annealing process
        /// </summary>
        protected virtual float InitialTemperature { get; set; } = 1000f;

        /// <summary>
        /// Gets or sets the cooling factor (should be close to but less than 1.0)
        /// </summary>
        protected virtual float CoolingFactor { get; set; } = 0.999991f;

        /// <summary>
        /// Gets or sets whether to display progress during execution
        /// </summary>
        protected virtual bool ShowProgress { get; set; } = true;

        /// <summary>
        /// Calculates the error/cost of the current solution
        /// </summary>
        public abstract float CalculateError();

        /// <summary>
        /// Loads data required for the problem
        /// </summary>
        public abstract void LoadData();

        /// <summary>
        /// Mutates the current solution
        /// </summary>
        public abstract void Mutate();

        /// <summary>
        /// Reverts the last mutation
        /// </summary>
        public abstract void RevertLastMutation();

        /// <summary>
        /// Called when a new best solution is found
        /// </summary>
        protected abstract void OnBestSolutionFound(float error);

        /// <summary>
        /// Displays the results after the algorithm completes
        /// </summary>
        protected abstract void DisplayResults();

        /// <summary>
        /// Runs the simulated annealing algorithm synchronously
        /// </summary>
        public virtual void Run()
        {
            LoadData();

            float oldError = CalculateError();
            float error = float.PositiveInfinity;
            bestErrorFound = float.PositiveInfinity;

            float temperature = InitialTemperature;

            for (int i = 0; i < Epochs; i++)
            {
                temperature *= CoolingFactor;
                Mutate();
                error = CalculateError();

                if (AcceptanceProbability(oldError, error, temperature) > (float)seed.NextDouble())
                {
                    oldError = error;
                }
                else
                {
                    RevertLastMutation();
                }

                if (error < bestErrorFound)
                {
                    bestErrorFound = error;
                    OnBestSolutionFound(error);
                }

                if (ShowProgress && i % 1000 == 0)
                {
                    Console.WriteLine($"Iteration {i}: old error: {oldError:F2} error: {error:F2} temperature: {temperature:F4}");
                }
            }

            DisplayResults();
        }

        /// <summary>
        /// Runs the simulated annealing algorithm asynchronously
        /// </summary>
        public virtual async Task RunAsync()
        {
            await Task.Run(() => Run());
        }

        /// <summary>
        /// Calculates the acceptance probability based on the Metropolis criterion
        /// </summary>
        protected float AcceptanceProbability(float oldError, float newError, float temperature)
        {
            return (float)Math.Exp((oldError - newError) / temperature);
        }

        /// <summary>
        /// Generates a random float between minimum and maximum
        /// </summary>
        public float GetRandomNumber(float minimum, float maximum)
        {
            return (float)seed.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
