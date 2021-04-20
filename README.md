# SimulatedAnnealing
A library of problems solved by Simulated Annealing in C#

Simulated Annealing in computing is nearly synonymous with the Traveling Salesperson Problem (TSP), in which a salesperson wants to visit as many cities as possible without repetition, and wants to make the trip as short as possible to reduce the cost through math.

The TSP problem is mathematically analogous to several other problems known as optimization problems https://en.wikipedia.org/wiki/Optimization_problem.

The typical formulation of the problem viewed through the lens of Simulated Annealing and other optimization techniques is to look through a search space and identify feasible solutions. For simple problems, we can sometimes find the best solution. For others, almost any approximate solution will do.

## What does this library do?
Currently this library applies Simulated Annealing to a clustering problem. Not only are we able to cluster better than with the k-means clustering algorithm, but we can make predictions as about what category an instance belongs to. For this project, I used the Fisher Iris data set from UCI's Machine Learning repository https://archive.ics.uci.edu/ml/datasets/iris.
