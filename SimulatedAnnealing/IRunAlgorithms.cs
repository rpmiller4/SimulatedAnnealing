namespace SimulatedAnnealing
{
    public interface IAnneal
    {
        float CalculateError();
        float GetRandomNumber(float minimum, float maximum);
        void LoadData();
        void RevertLastMutation();
        void Run();
        void Mutate();
    }
}