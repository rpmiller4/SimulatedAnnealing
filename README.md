# SimulatedAnnealing

A well-structured C# library for solving optimization problems using Simulated Annealing and other algorithms.

## Overview

Simulated Annealing (SA) is a probabilistic optimization technique inspired by the annealing process in metallurgy. This library provides a clean, extensible framework for applying SA to various optimization problems.

The library demonstrates SA applications on:
- **Traveling Salesperson Problem (TSP)**: Find optimal routes visiting cities
- **Iris Data Clustering**: Classify iris flowers (outperforms k-means)
- **Vehicle Routing Problem (VRP)**: Multi-vehicle routing with time window constraints
- **Branch & Bound**: Exact TSP solver for comparison

## Features

✨ **Clean Architecture**
- Clear separation of concerns with Models, Problems, Core, and Utilities
- One class per file for better maintainability
- Consistent namespace organization

🔧 **Extensible Design**
- Abstract `SimulatedAnnealingBase` class for easy problem implementation
- Consistent interface contracts (`IRun`, `IAnneal`)
- Template Method pattern for algorithm customization

♻️ **DRY Principles**
- No code duplication
- Centralized utilities and configuration
- Reusable distance calculations

⚡ **Async/Await Support**
- Both synchronous and asynchronous execution
- Proper async implementations throughout

📝 **Well-Documented**
- XML documentation on public APIs
- Comprehensive refactoring summary
- Clear usage examples

## Project Structure

```
SimulatedAnnealing/
├── Core/                          # Core framework
│   ├── SimulatedAnnealingBase.cs  # Base class for SA algorithms
│   ├── DataConfiguration.cs       # Configuration management
│   ├── IAnneal.cs                 # SA interface
│   └── IRun.cs                    # Runnable algorithm interface
├── Models/                        # Data models
│   ├── City.cs
│   ├── Iris.cs
│   ├── TimeWindow.cs
│   ├── Appointment.cs
│   ├── PathAndCost.cs
│   └── VehicleSchedule.cs
├── Problems/                      # Algorithm implementations
│   ├── TravelingSalespersonProblem.cs
│   ├── IrisDataCluster.cs
│   ├── VehicleRoutingProblem.cs
│   └── BranchAndBound.cs
├── Utilities/                     # Helper utilities
│   ├── DistanceCalculator.cs
│   └── MatrixHelper.cs
└── Program.cs                     # Example usage
```

## Quick Start

### Traveling Salesperson Problem

```csharp
using SimulatedAnnealing.Problems;
using SimulatedAnnealing.Core;

// Generate random cities
var tsp = new TravelingSalespersonProblem(50);
tsp.Run();

// Or load from file
var tsp = new TravelingSalespersonProblem(DataConfiguration.CitiesDataFile);
tsp.Run();

// Apply 2-opt optimization for improvement
tsp.RunTwoOpt();
```

### Iris Clustering

```csharp
using SimulatedAnnealing.Problems;

var classifier = new IrisDataCluster(clusters: 3);
await classifier.RunAsync();

// Predict new iris classification
var newIris = new Iris {
    SepalLength = 5.0f,
    SepalWidth = 3.5f,
    PetalLength = 1.6f,
    PetalWidth = 0.6f
};
int predictedClass = classifier.PredictClass(newIris);
```

### Vehicle Routing Problem

```csharp
using SimulatedAnnealing.Problems;

var vrp = new VehicleRoutingProblem(vehicles: 3);
vrp.Run();
```

### Branch and Bound (Exact Algorithm)

```csharp
using SimulatedAnnealing.Problems;

var bnb = new BranchAndBound(DataConfiguration.CitiesDataFile);
await bnb.RunAsync();
```

## Configuration

Customize data file paths:

```csharp
using SimulatedAnnealing.Core;

// Set custom data directories
DataConfiguration.BaseDataPath = "./mydata";
DataConfiguration.BaseSynthDataPath = "./mysynthdata";

// Or use predefined paths
string irisFile = DataConfiguration.IrisDataFile;
string citiesFile = DataConfiguration.CitiesDataFile;
```

## Creating Custom Problems

Extend `SimulatedAnnealingBase` to create new optimization problems:

```csharp
using SimulatedAnnealing.Core;

public class MyOptimizationProblem : SimulatedAnnealingBase
{
    // Configure SA parameters
    protected override int Epochs { get; set; } = 1000000;
    protected override float InitialTemperature { get; set; } = 1000f;
    protected override float CoolingFactor { get; set; } = 0.999991f;

    // Implement required methods
    public override void LoadData()
    {
        // Load your problem data
    }

    public override void Mutate()
    {
        // Mutate current solution
    }

    public override void RevertLastMutation()
    {
        // Undo last mutation
    }

    public override float CalculateError()
    {
        // Calculate solution cost/error
        return 0f;
    }

    protected override void OnBestSolutionFound(float error)
    {
        // Track best solution
    }

    protected override void DisplayResults()
    {
        // Display final results
    }
}
```

## Algorithm Parameters

### Simulated Annealing Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `Epochs` | 400,000 | Number of iterations |
| `InitialTemperature` | 1000.0 | Starting temperature |
| `CoolingFactor` | 0.999991 | Temperature reduction rate |
| `ShowProgress` | true | Display iteration progress |

Different problems use different values:
- **TSP**: 400k epochs, T=1000, cooling=0.999991
- **Iris**: 2M epochs, T=1, cooling=0.99999
- **VRP**: 1.4M epochs, T=1000, cooling=0.999992

## Distance Metrics

The library provides multiple distance calculations via `DistanceCalculator`:

```csharp
using SimulatedAnnealing.Utilities;
using SimulatedAnnealing.Models;

City a = new City { X = 0, Y = 0 };
City b = new City { X = 3, Y = 4 };

float manhattan = DistanceCalculator.CalculateManhattanDistance(a, b);  // L1: |dx| + |dy|
float euclidean = DistanceCalculator.CalculateEuclideanDistance(a, b);  // L2: sqrt(dx² + dy²)
```

## Testing

Run the unit tests:

```bash
dotnet test
```

Current test coverage includes:
- Distance calculations
- Branch and Bound algorithm (sync and async)

## What's New (Recent Refactoring)

This library underwent comprehensive refactoring to improve design and maintainability:

✅ Eliminated ~500 lines of code duplication
✅ Fixed all interface implementation issues
✅ Organized code into proper directory structure
✅ Centralized configuration management
✅ Improved test coverage and naming
✅ Added comprehensive documentation

See [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md) for detailed changes.

## Requirements

- .NET Core 3.1+ or .NET 5+
- CsvHelper library (for data loading)
- NUnit (for testing)

## Data Files

The library includes sample data:
- `data/iris.data` - Fisher Iris dataset
- `data/cities.data` - City coordinates for TSP
- `synthdata/locations.data` - VRP locations
- `synthdata/timeWindows.data` - VRP time constraints
- `synthdata/timeDistances.data` - VRP travel times

## Further Reading

- [Simulated Annealing - Wikipedia](https://en.wikipedia.org/wiki/Simulated_annealing)
- [Traveling Salesman Problem](https://en.wikipedia.org/wiki/Travelling_salesman_problem)
- [Vehicle Routing Problem](https://en.wikipedia.org/wiki/Vehicle_routing_problem)
- [2-opt Algorithm](https://en.wikipedia.org/wiki/2-opt)
- [Branch and Bound](https://en.wikipedia.org/wiki/Branch_and_bound)

## License

See LICENSE file for details.

## Contributing

Contributions welcome! Please:
1. Follow the existing code structure
2. Add tests for new features
3. Update documentation
4. Ensure all tests pass

## Acknowledgments

- Fisher Iris dataset from [UCI Machine Learning Repository](https://archive.ics.uci.edu/ml/datasets/iris)
- Branch and Bound visualization reference: [tspvis.com](https://tspvis.com/)
