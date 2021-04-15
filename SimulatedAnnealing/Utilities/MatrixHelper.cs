using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SimulatedAnnealing.Utilities
{
    public class MatrixHelper
    {
        public MatrixHelper()
        {

        }

        public int[][] GetRecords(string path)
        {
            List<List<int>> records = new List<List<int>>();
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    records.Add(line.Split(" ").Select(x=>Convert.ToInt32(x)).ToList());
                }
            }

            int[][] recordsArray = new int[records.Count][];

            for (int i = 0; i < recordsArray.Length; i++)
            {
                recordsArray[i] = records[i].ToArray();
            }

            return recordsArray;
        }
    }
}
