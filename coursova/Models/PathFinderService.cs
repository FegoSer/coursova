using System;
using System.Collections.Generic;
using System.Text;

namespace Coursova.Models
{
    public class PathFinderResult
    {
        public int Distance { get; set; }
        public required List<int> Path { get; set; }
        public required List<(int, int)> Edges { get; set; }
        public int OperationsCount { get; set; }
        public required string Message { get; set; }
    }

    public class PathFinderService
    {
        public PathFinderResult FindPath(int[,] weights, int startVertex, int endVertex, string selectedMethod)
        {
            try
            {
                if (startVertex == endVertex)
                {
                    return new PathFinderResult
                    {
                        Distance = 0,
                        Path = new List<int> { startVertex },
                        Edges = new List<(int, int)>(),
                        OperationsCount = 1,
                        Message = $"Мінімальна відстань: 0\nКількість операцій: 1\nШлях: {startVertex + 1}"
                    };
                }

                Algorithm algorithm = selectedMethod == Constants.FloydWarshallAlgorithmName
                    ? new FloydWarshall()
                    : new Dantzig();

                var (distance, path, edges, operations) = algorithm.FindPath(weights, startVertex, endVertex);

                string message;
                if (distance == int.MaxValue)
                {
                    message = "Шлях між вершинами не існує.";
                }
                else
                {
                    var pathDescription = new StringBuilder();
                    if (path != null && path.Count > 0)
                    {
                        pathDescription.Append("Шлях: ");
                        for (int i = 0; i < path.Count; i++)
                        {
                            pathDescription.Append(path[i] + 1);
                            if (i < path.Count - 1)
                                pathDescription.Append(" → ");
                        }
                    }
                    message = $"Мінімальна відстань: {distance}\nКількість операцій: {operations}\n{pathDescription}";
                }

                return new PathFinderResult
                {
                    Distance = distance,
                    Path = path ?? [],
                    Edges = edges,
                    OperationsCount = operations,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка пошуку шляху: {ex.Message}");
            }
        }
    }
}