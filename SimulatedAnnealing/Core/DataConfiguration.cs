using System.IO;

namespace SimulatedAnnealing.Core
{
    /// <summary>
    /// Configuration class for managing data file paths
    /// </summary>
    public static class DataConfiguration
    {
        private static string _baseDataPath = "./data";
        private static string _baseSynthDataPath = "./synthdata";

        /// <summary>
        /// Gets or sets the base directory for data files
        /// </summary>
        public static string BaseDataPath
        {
            get => _baseDataPath;
            set => _baseDataPath = value;
        }

        /// <summary>
        /// Gets or sets the base directory for synthetic data files
        /// </summary>
        public static string BaseSynthDataPath
        {
            get => _baseSynthDataPath;
            set => _baseSynthDataPath = value;
        }

        /// <summary>
        /// Gets the full path for a data file
        /// </summary>
        public static string GetDataFilePath(string filename)
        {
            return Path.Combine(BaseDataPath, filename);
        }

        /// <summary>
        /// Gets the full path for a synthetic data file
        /// </summary>
        public static string GetSynthDataFilePath(string filename)
        {
            return Path.Combine(BaseSynthDataPath, filename);
        }

        // Predefined data file paths
        public static string IrisDataFile => GetDataFilePath("iris.data");
        public static string CitiesDataFile => GetDataFilePath("cities.data");
        public static string CoordsDataFile => GetDataFilePath("coords.txt");
        public static string LocationsDataFile => GetSynthDataFilePath("locations.data");
        public static string TimeWindowsDataFile => GetSynthDataFilePath("timeWindows.data");
        public static string TimeDistancesDataFile => GetSynthDataFilePath("timeDistances.data");
    }
}
