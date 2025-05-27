using Avalonia.Media;

namespace Coursova.Models
{
    public static class Constants
    {
        public const string HelloMessage = "Ця програма призначена для знаходження найкоротшого шляху у графі за допомогою алгоритму Флойда-Воршела або Данцига.";

        public const string SolutionFilePath = "./solution.txt";

        public const string FloydWarshallAlgorithmName = "Флойд-Воршел";
        public const string DantzigAlgorithmName = "Данциг";

        public const int DefaultMatrixSize = 5;
        public const int DefaultStartVertex = 1;
        public const int DefaultEndVertex = 2;
        public const int MinGraphSize = 1;
        public const int MaxGraphSize = 10;
        public const double MinEdgeWeight = 1;
        public const double MinRandomEdgeWeight = 1;
        public const double MaxEdgeWeight = 1000;
        public const double MaxRandomEdgeWeight = 10;
        public const double EdgeExistProbability = 0.7;

        public const int NodeRadius = 20;
        public const int NodeLabelFontSize = 14;
        public const double ArrowSize = 10.0;
        public const double CurveOffset = 50.0;
        public static readonly Color RedColor = Colors.Red;
        public static readonly Color GrayColor = Colors.Gray;
        public static readonly Color BlackColor = Colors.Black;
        public static readonly IBrush RegularNodeFill = Brushes.LightBlue;
        public static readonly IBrush RegularNodeStroke = Brushes.Navy;
        public static readonly IBrush HighlightNodeStroke = Brushes.Red;
    }
}