using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SimulatedAnnealing
{
    public class IrisDataCluster
    {
        private List<Iris> irisRecords = new List<Iris>();
        private List<Iris> irisClassifiers = new List<Iris>();
        private Random seed = new Random();
        private int initialClusters;

        private Iris oldIris;
        private Iris oldClassifier;
        private int irisIndexToMutate;
        private int classifierIndexToMutate;

        public IrisDataCluster(int initialClusters = 3)
        {
            this.initialClusters = initialClusters;
        }

        // deserialize data into objects
        public void LoadData()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = "\n"
            };

            using (var reader = new StreamReader("data/iris.data"))
            using (var csv = new CsvReader(reader, config))
            {
                irisRecords = csv.GetRecords<Iris>().ToList();
            }
        }

        public void Run()
        {
            LoadData();
            SetupClusterIndicators();
            float oldError = CalculateError();
            float error = float.PositiveInfinity;

            int epochs = 2000000;
            float temperature = 1f;
            float coolingFactor = .99999f;

            for (int i = 0; i < epochs; i++)
            {
                temperature *= coolingFactor;
                Step();
                error = CalculateError();
                if (temperature > (float)seed.NextDouble()) // keep solution
                {
                    oldError = error;
                }
                else if (error < oldError) // keep solution
                {
                    oldError = error;
                }
                else
                {
                    RevertLastStep();
                }

                Console.WriteLine($"old error: {Math.Sqrt(oldError)} error: {Math.Sqrt(error)} temperature: {temperature}");
            }


            Console.WriteLine("classification label, original label");
            for (int i = 0; i < irisRecords.Count(); i++)
            {
                Console.WriteLine($"{irisRecords[i].ClassificationLabel}, {irisRecords[i].OriginalCategory} ");
            }
            Iris irisToClassify = new Iris
            {//5.0,3.5,1.6,0.6
                OriginalCategory = "Should be a setosa",
                SepalLength = 5f,
                SepalWidth = 3.5f,
                PetalLength = 1.6f,
                PetalWidth = 0.6f
            };
            int predictedClass = PredictClass(irisToClassify);
            Console.WriteLine($"predicted Class for last iris: {predictedClass}, {irisToClassify.ClassificationLabel}");
        }

        public void Step()
        {
            MutateIris();
            MutateClassifier(1);
        }

        public void RevertLastStep()
        {
            irisClassifiers[classifierIndexToMutate] = oldClassifier;
            irisRecords[irisIndexToMutate] = oldIris;
        }

        public float CalculateError()
        {
            List<Iris> matchingIrises;
            float squaredSumError = 0;

            foreach (var c in irisClassifiers)
            {
                //find all matching irises;
                matchingIrises = irisRecords.Where(x => x.ClassificationLabel == c.ClassificationLabel).ToList();
                for (int i = 0; i < matchingIrises.Count; i++)
                {
                    squaredSumError += CalculateCategoryDistance(c, matchingIrises[i]);
                }
            }
            return squaredSumError;
        }

        public float CalculateCategoryDistance(Iris classifier, Iris record)
        {
            float sumOfPropDistances = 0;
            sumOfPropDistances += (float)Math.Pow(classifier.PetalLength - record.PetalLength, 2);
            sumOfPropDistances += (float)Math.Pow(classifier.PetalWidth - record.PetalWidth, 2);
            sumOfPropDistances += (float)Math.Pow(classifier.SepalLength - record.SepalLength, 2);
            sumOfPropDistances += (float)Math.Pow(classifier.SepalWidth - record.SepalWidth, 2);
            return sumOfPropDistances;
        }

        private void MutateIris()
        {
            int irisCount = irisRecords.Count;
            int classifiersCount = irisClassifiers.Count;
            irisIndexToMutate = seed.Next(irisCount);
            Iris toMutate = irisRecords[irisIndexToMutate];
            oldIris = toMutate.Copy(toMutate);

            toMutate.ClassificationLabel = (toMutate.ClassificationLabel + seed.Next(classifiersCount)) % classifiersCount;
        }

        private void MutateClassifier(float mutationDistance)
        {
            int classifiersCount = irisClassifiers.Count;
            classifierIndexToMutate = seed.Next(classifiersCount);
            Iris toMutate = irisClassifiers[classifierIndexToMutate];
            oldClassifier = toMutate.Copy(toMutate);

            switch (seed.Next(4))
            {
                case 0:
                    toMutate.PetalLength += GetRandomNumber(-mutationDistance, mutationDistance);
                    break;
                case 1:
                    toMutate.PetalWidth += GetRandomNumber(-mutationDistance, mutationDistance);
                    break;
                case 2:
                    toMutate.SepalLength += GetRandomNumber(-mutationDistance, mutationDistance);
                    break;
                case 3:
                    toMutate.SepalWidth += GetRandomNumber(-mutationDistance, mutationDistance);
                    break;
            }
        }

        public void SetupClusterIndicators()
        {
            for (int i = 0; i < initialClusters; i++)
            {
                irisClassifiers.Add(new Iris
                {
                    ClassificationLabel = i,
                    OriginalCategory = $"{i}",
                    PetalLength = seed.Next(10),
                    PetalWidth = seed.Next(10),
                    SepalLength = seed.Next(10),
                    SepalWidth = seed.Next(10)
                });
            }
        }

        public int PredictClass(Iris irisToClassify)
        {
            int predictedClass = int.MaxValue;
            float bestError = float.MaxValue;
            irisRecords.Add(irisToClassify);

            for (int i = 0; i < irisClassifiers.Count; i++)
            {
                irisRecords[^1].ClassificationLabel = irisClassifiers[i].ClassificationLabel;
                float error = CalculateError();
                if (error < bestError)
                {
                    predictedClass = irisRecords[^1].ClassificationLabel;
                    bestError = error;
                }
            }
            irisRecords.RemoveAt(irisRecords.Count - 1);
            return predictedClass;
        }

        public float GetRandomNumber(float minimum, float maximum)
        {
            return (float)seed.NextDouble() * (maximum - minimum) + minimum;
        }

    }

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
