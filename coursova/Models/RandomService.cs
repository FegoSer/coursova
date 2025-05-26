using System;
using System.Text;

namespace Coursova.Models
{
    public class RandomService
    {
        private readonly Random _rnd = new Random();

        public string GenerateRandomMatrixText(string sizeInput)
        {
            if (!int.TryParse(sizeInput, out int size) || size <= 0)
            {
                throw new ArgumentException("Некоректна розмірність.");
            }

            if (size < Constants.MinGraphSize || size > Constants.MaxGraphSize)
            {
                throw new ArgumentException($"Розмірність графа повинна бути від {Constants.MinGraphSize} до {Constants.MaxGraphSize}.");
            }

            var sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                var rowSb = new StringBuilder();
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                    {
                        rowSb.Append("0");
                    }
                    else
                    {
                        int weight = _rnd.NextDouble() < Constants.EdgeExistProbability ?
                            _rnd.Next((int)Constants.MinRandomEdgeWeight, (int)Constants.MaxRandomEdgeWeight + 1) : 0;
                        rowSb.Append(weight);
                    }

                    if (j < size - 1)
                        rowSb.Append(" ");
                }
                sb.Append(rowSb.ToString());
                if (i < size - 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}