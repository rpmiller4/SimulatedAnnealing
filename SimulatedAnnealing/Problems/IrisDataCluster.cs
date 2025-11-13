using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SimulatedAnnealing.Core;
using SimulatedAnnealing.Models;

namespace SimulatedAnnealing.Problems
{
    public class IrisDataCluster : SimulatedAnnealingBase
    {
        private List<Iris> irisRecords = new List<Iris>();
        private List<Iris> irisClassifiers = new List<Iris>();
        private int initialClusters;

        private Iris oldIris;
        private Iris oldClassifier;
        private int irisIndexToMutate;
        private int classifierIndexToMutate;

        private string dataFilePath;

        public IrisDataCluster(int initialClusters = 3, string dataFilePath = null)
        {
            this.initialClusters = initialClusters;
            this.dataFilePath = dataFilePath ?? DataConfiguration.IrisDataFile;

            // Override default parameters for Iris clustering
            Epochs = 2000000;
            InitialTemperature = 1f;
            CoolingFactor = 0.99999f;
        }

        public override void LoadData()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine
            };

            using (var reader = new StreamReader(dataFilePath))
            using (var csv = new CsvReader(reader, config))
            {
                irisRecords = csv.GetRecords<Iris>().ToList();
            }

            SetupClusterIndicators();
        }

        private void SetupClusterIndicators()
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

        public override void Mutate()
        {
            MutateIris();
            MutateClassifier(1);
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

        public override void RevertLastMutation()
        {
            irisClassifiers[classifierIndexToMutate] = oldClassifier;
            irisRecords[irisIndexToMutate] = oldIris;
        }

        public override float CalculateError()
        {
            List<Iris> matchingIrises;
            float squaredSumError = 0;

            foreach (var c in irisClassifiers)
            {
                matchingIrises = irisRecords.Where(x => x.ClassificationLabel == c.ClassificationLabel).ToList();
                for (int i = 0; i < matchingIrises.Count; i++)
                {
                    squaredSumError += CalculateCategoryDistance(c, matchingIrises[i]);
                }
            }
            return squaredSumError;
        }

        private float CalculateCategoryDistance(Iris classifier, Iris record)
        {
            float sumOfPropDistances = 0;
            sumOfPropDistances += (float)Math.Pow(classifier.PetalLength - record.PetalLength, 2);
            sumOfPropDistances += (float)Math.Pow(classifier.PetalWidth - record.PetalWidth, 2);
            sumOfPropDistances += (float)Math.Pow(classifier.SepalLength - record.SepalLength, 2);
            sumOfPropDistances += (float)Math.Pow(classifier.SepalWidth - record.SepalWidth, 2);
            return sumOfPropDistances;
        }

        protected override void OnBestSolutionFound(float error)
        {
            // Best solution is automatically tracked in the mutable irisRecords and irisClassifiers
        }

        protected override void DisplayResults()
        {
            Console.WriteLine("classification label, original label");
            for (int i = 0; i < irisRecords.Count(); i++)
            {
                Console.WriteLine($"{irisRecords[i].ClassificationLabel}, {irisRecords[i].OriginalCategory} ");
            }

            // Test prediction
            Iris irisToClassify = new Iris
            {
                OriginalCategory = "Should be a setosa",
                SepalLength = 5f,
                SepalWidth = 3.5f,
                PetalLength = 1.6f,
                PetalWidth = 0.6f
            };
            int predictedClass = PredictClass(irisToClassify);
            Console.WriteLine($"predicted Class for test iris: {predictedClass}, {irisToClassify.ClassificationLabel}");
        }

        /// <summary>
        /// Predicts the classification label for a new iris
        /// </summary>
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
    }
}
