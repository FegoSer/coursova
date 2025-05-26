using System.Text;
using System.Collections.Generic;
using System;
using System.IO;

namespace Coursova.Models
{
    public class FileService
    {
        public void SaveToFile(string matrixText, string selectedMethod,
                               string startVertexForDisplay, string endVertexForDisplay,
                               int distance, List<int> shortestPath, int operationsCount)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Матриця вагів (орієнтований граф):");
                sb.AppendLine(matrixText);
                sb.AppendLine();

                sb.AppendLine($"Метод пошуку: {selectedMethod}");
                sb.AppendLine($"Пошук шляху від вершини {startVertexForDisplay} до вершини {endVertexForDisplay}.");
                sb.AppendLine();

                if (distance == int.MaxValue)
                {
                    sb.AppendLine("Результат: Шлях між вказаними вершинами не існує.");
                }
                else
                {
                    sb.AppendLine($"Мінімальна відстань: {distance}");
                    sb.AppendLine($"Кількість операцій: {operationsCount}");

                    if (shortestPath != null && shortestPath.Count > 0)
                    {
                        sb.Append("Послідовність вершин шляху: ");
                        List<string> userFriendlyPath = new List<string>();
                        foreach (var node in shortestPath)
                        {
                            userFriendlyPath.Add((node + 1).ToString());
                        }
                        sb.AppendLine(string.Join(" -> ", userFriendlyPath));
                    }
                    else if (startVertexForDisplay == endVertexForDisplay && distance == 0)
                    {
                        sb.AppendLine($"Послідовність вершин шляху: {startVertexForDisplay}");
                    }
                    else
                    {
                        sb.AppendLine("Деталі шляху не вдалося визначити.");
                    }
                }

                File.WriteAllText(Constants.SolutionFilePath, sb.ToString());
            }
            catch (IOException ioEx)
            {
                throw new Exception("Помилка запису у файл: " + ioEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Загальна помилка при збереженні: " + ex.Message, ex);
            }
        }
    }
}