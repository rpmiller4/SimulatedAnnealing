namespace SimulatedAnnealing.Models
{
    public class Iris
    {
        public float SepalLength { get; set; }
        public float SepalWidth { get; set; }
        public float PetalLength { get; set; }
        public float PetalWidth { get; set; }
        public string OriginalCategory { get; set; }
        public int ClassificationLabel { get; set; }

        public Iris Copy(Iris toCopy)
        {
            return new Iris
            {
                ClassificationLabel = toCopy.ClassificationLabel,
                OriginalCategory = toCopy.OriginalCategory,
                PetalLength = toCopy.PetalLength,
                PetalWidth = toCopy.PetalWidth,
                SepalLength = toCopy.SepalLength,
                SepalWidth = toCopy.SepalWidth
            };
        }
    }
}
