using System.Collections.ObjectModel;
using System.Text;
using System;

namespace Coursova.Models
{
    public class MatrixService
    {
        private void ValidateSize(int size)
        {
            if (size < Constants.MinGraphSize || size > Constants.MaxGraphSize)
            {
                throw new ArgumentException($"Розмірність графа повинна бути від {Constants.MinGraphSize} до {Constants.MaxGraphSize}.");
            }
        }

        private void ValidateWeight(int weight, int i, int j)
        {
            if (i == j && weight != 0)
            {
                throw new ArgumentException($"Вага петлі (ребра з вершини в себе) повинна бути 0, знайдено {weight} у позиції [{i + 1}, {j + 1}].");
            }

            if (i != j && weight != 0 && (weight < Constants.MinEdgeWeight || weight > Constants.MaxEdgeWeight))
            {
                throw new ArgumentException($"Вага ребра повинна бути від {Constants.MinEdgeWeight} до {Constants.MaxEdgeWeight}, знайдено {weight} у позиції [{i + 1}, {j + 1}].");
            }
        }

        public int[,] ParseMatrixFromText(string matrixText, string sizeInput)
        {
            if (!int.TryParse(sizeInput, out int size) || size <= 0)
            {
                throw new ArgumentException("Помилка: розмірність повинна бути більшою за 0.");
            }

            ValidateSize(size);

            var weights = new int[size, size];
            string[] lines = matrixText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length != size)
            {
                throw new ArgumentException($"Помилка: Кількість рядків ({lines.Length}) не відповідає заданій розмірності ({size}).");
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != size)
                {
                    throw new ArgumentException($"Помилка: кількість значень у рядку {i + 1} ({values.Length}) не відповідає заданій розмірності ({size}).");
                }

                for (int j = 0; j < values.Length; j++)
                {
                    if (!int.TryParse(values[j], out weights[i, j]))
                    {
                        throw new ArgumentException($"Помилка: некоректне значення '{values[j]}' у позиції [{i + 1}, {j + 1}].");
                    }
                    ValidateWeight(weights[i, j], i, j);
                }
            }
            return weights;
        }

        public string CreateEmptyMatrix(string sizeInput)
        {
            if (!int.TryParse(sizeInput, out int size) || size <= 0)
            {
                throw new ArgumentException("Некоректна розмірність.");
            }

            ValidateSize(size);

            var sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    sb.Append("0");
                    if (j < size - 1)
                        sb.Append(" ");
                }
                if (i < size - 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }

        public (int start, int end) ValidateStartEndVertices(string startVertex, string endVertex, string sizeInput)
        {
            try
            {
                if (!int.TryParse(startVertex, out int start))
                {
                    throw new ArgumentException($"Помилка: початкова вершина '{startVertex}' не є цілим числом.");
                }
                start -= 1;

                if (!int.TryParse(endVertex, out int end))
                {
                    throw new ArgumentException($"Помилка: кінцева вершина '{endVertex}' не є числом.");
                }
                end -= 1;

                int size = 0;
                if (int.TryParse(sizeInput, out int parsedSizeInput) && parsedSizeInput > 0)
                {
                    size = parsedSizeInput;
                    ValidateSize(size);
                }

                if (size > 0)
                {
                    if (start < 0 || start >= size || end < 0 || end >= size)
                    {
                        throw new ArgumentException($"Помилка: Некоректно вказані вершини для графа. Вони повинні бути в діапазоні від 1 до {size}.");
                    }
                }

                return (start, end);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Помилка формату при парсингу вершин.");
            }
        }

        public ObservableCollection<ObservableCollection<string>> ConvertMatrixTextToObservableCollection(string matrixText)
        {
            var matrix = new ObservableCollection<ObservableCollection<string>>();
            if (string.IsNullOrWhiteSpace(matrixText)) return matrix;

            string[] lines = matrixText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                var rowCollection = new ObservableCollection<string>();
                string[] values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string value in values)
                {
                    rowCollection.Add(value);
                }
                matrix.Add(rowCollection);
            }
            return matrix;
        }
    }
}