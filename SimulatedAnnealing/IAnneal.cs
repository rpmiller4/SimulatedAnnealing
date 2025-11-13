namespace SimulatedAnnealing
{
    public interface IAnneal : IRun
    {
        float CalculateError();
        float GetRandomNumber(float minimum, float maximum);
        void LoadData();
        void RevertLastMutation();
        void Mutate();
    }
}