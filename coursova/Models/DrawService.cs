using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.Collections.Generic;
using System;

namespace Coursova.Models
{
    public class DrawService(Canvas graphCanvas, MainWindowViewModel viewModel)
    {
        private readonly Canvas _graphCanvas = graphCanvas;
        private readonly MainWindowViewModel ViewModel = viewModel;

        public void DrawGraph()
        {
            if (_graphCanvas == null)
            {
                return;
            }

            _graphCanvas.Children.Clear();

            if (ViewModel == null)
            {
                return;
            }

            var weights = ViewModel.GetWeightsMatrix();
            if (weights == null)
            {
                return;
            }
            int size = weights.GetLength(0);
            if (size <= 0)
            {
                return;
            }

            var (startVertex, endVertex) = ViewModel.GetStartEndVertices();
            var shortestPath = ViewModel.ShortestPath;
            Dictionary<(int, int), bool> isEdgeInPath = [];
            if (shortestPath != null && shortestPath.Count > 1)
            {
                for (int k = 0; k < shortestPath.Count - 1; k++)
                {
                    isEdgeInPath[(shortestPath[k], shortestPath[k + 1])] = true;
                }
            }

            var nodePositions = new (double X, double Y)[size];
            double canvasWidth = _graphCanvas.Bounds.Width;
            double canvasHeight = _graphCanvas.Bounds.Height;

            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                return;
            }

            double centerX = canvasWidth / 2.0;
            double centerY = canvasHeight / 2.0;
            double radius = Math.Min(centerX, centerY) - Constants.NodeRadius - 20.0;
            if (radius <= 0.0)
            {
                radius = Math.Min(canvasWidth, canvasHeight) / 3.0;
            }

            for (int k = 0; k < size; k++)
            {
                double angle = 2.0 * Math.PI * (double)k / (double)size;
                nodePositions[k] = (centerX + radius * Math.Cos(angle), centerY + radius * Math.Sin(angle));
            }

            for (int k = 0; k < size; k++)
            {
                bool isHighlighted = (k == startVertex || k == endVertex);
                DrawNode(nodePositions[k], k, isHighlighted);
            }

            var drawnEdgeShapes = new Dictionary<(int from, int to), Avalonia.Controls.Shapes.Shape>();

            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    bool edgeIToJExists = weights[i, j] > 0;
                    bool edgeJToIExists = weights[j, i] > 0;

                    if (!edgeIToJExists && !edgeJToIExists)
                    {
                        continue;
                    }

                    if (edgeIToJExists && edgeJToIExists)
                    {
                        var dxGlobal = nodePositions[j].X - nodePositions[i].X;
                        var dyGlobal = nodePositions[j].Y - nodePositions[i].Y;
                        var lengthGlobal = Math.Sqrt(dxGlobal * dxGlobal + dyGlobal * dyGlobal);
                        if (lengthGlobal == 0)
                        {
                            continue;
                        }

                        var unitXGlobal = dxGlobal / lengthGlobal;
                        var unitYGlobal = dyGlobal / lengthGlobal;

                        double curveOffsetXIToJ = -unitYGlobal * Constants.CurveOffset;
                        double curveOffsetYIToJ = unitXGlobal * Constants.CurveOffset;

                        var shapeIToJ = DrawEdge(nodePositions[i], nodePositions[j], weights[i, j], false, curveOffsetXIToJ, curveOffsetYIToJ, true);
                        if (shapeIToJ != null) drawnEdgeShapes[(i, j)] = shapeIToJ;

                        var shapeJToI = DrawEdge(nodePositions[j], nodePositions[i], weights[j, i], false, 0.0, 0.0, true);
                        if (shapeJToI != null) drawnEdgeShapes[(j, i)] = shapeJToI;
                    }
                    else if (edgeIToJExists)
                    {
                        var shape = DrawEdge(nodePositions[i], nodePositions[j], weights[i, j], false, 0.0, 0.0, true);
                        if (shape != null) drawnEdgeShapes[(i, j)] = shape;
                    }
                    else if (edgeJToIExists)
                    {
                        var shape = DrawEdge(nodePositions[j], nodePositions[i], weights[j, i], false, 0.0, 0.0, true);
                        if (shape != null) drawnEdgeShapes[(j, i)] = shape;
                    }
                }
            }

            if (ViewModel.PathEdges != null)
            {
                foreach (var edgeInPath in ViewModel.PathEdges)
                {
                    if (drawnEdgeShapes.TryGetValue((edgeInPath.Item1, edgeInPath.Item2), out var shapeToRecolor))
                    {
                        shapeToRecolor.Stroke = new SolidColorBrush(Constants.RedColor);
                    }
                }
            }
        }

        private void DrawNode(in (double X, double Y) position, int nodeId, bool isHighlighted)
        {
            var nodeCircle = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = (double)Constants.NodeRadius * 2.0,
                Height = (double)Constants.NodeRadius * 2.0,
                Fill = Constants.RegularNodeFill,
                Stroke = isHighlighted ? Constants.HighlightNodeStroke : Constants.RegularNodeStroke,
                StrokeThickness = isHighlighted ? 3.0 : 2.0
            };

            Canvas.SetLeft(nodeCircle, position.X - (double)Constants.NodeRadius);
            Canvas.SetTop(nodeCircle, position.Y - (double)Constants.NodeRadius);
            _graphCanvas.Children.Add(nodeCircle);

            var nodeLabel = new TextBlock
            {
                Text = (nodeId + 1).ToString(),
                FontSize = (double)Constants.NodeLabelFontSize,
                FontWeight = FontWeight.Bold,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var labelWidth = (double)Constants.NodeLabelFontSize;
            var labelHeight = (double)Constants.NodeLabelFontSize;
            Canvas.SetLeft(nodeLabel, position.X - labelWidth / 2.0);
            Canvas.SetTop(nodeLabel, position.Y - labelHeight / 2.0);
            _graphCanvas.Children.Add(nodeLabel);
        }

        private Avalonia.Controls.Shapes.Shape? DrawEdge(in (double X, double Y) from, in (double X, double Y) to,
                             int weight, bool isInShortestPath,
                             double controlPointOffsetX, double controlPointOffsetY,
                             bool isPreDrawingPhase)
        {
            bool isCurved = controlPointOffsetX != 0.0 || controlPointOffsetY != 0.0;

            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);
            if (length == 0)
            {
                return null;
            }

            var unitX = dx / length;
            var unitY = dy / length;

            var startX = from.X + unitX * (double)Constants.NodeRadius;
            var startY = from.Y + unitY * (double)Constants.NodeRadius;
            var endX = to.X - unitX * (double)Constants.NodeRadius;
            var endY = to.Y - unitY * (double)Constants.NodeRadius;

            double midPointX = (from.X + to.X) / 2.0;
            double midPointY = (from.Y + to.Y) / 2.0;

            double labelPosX, labelPosY;

            Avalonia.Controls.Shapes.Shape edgeShape;

            if (isCurved)
            {
                double controlX = midPointX + controlPointOffsetX;
                double controlY = midPointY + controlPointOffsetY;

                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure { StartPoint = new Avalonia.Point(startX, startY) };
                var bezierSegment = new QuadraticBezierSegment
                {
                    Point1 = new Avalonia.Point(controlX, controlY),
                    Point2 = new Avalonia.Point(endX, endY)
                };
                if (pathFigure.Segments == null)
                {
                    return null;
                }
                pathFigure.Segments.Add(bezierSegment);

                if (pathGeometry.Figures == null)
                {
                    return null;
                }
                pathGeometry.Figures.Add(pathFigure);

                var currentPath = new Avalonia.Controls.Shapes.Path
                {
                    Data = pathGeometry,
                    Stroke = new SolidColorBrush(Constants.GrayColor),
                    StrokeThickness = isInShortestPath ? 2.5 : 1.5
                };
                _graphCanvas.Children.Add(currentPath);
                edgeShape = currentPath;

                bool detailsShouldBeColoredAccordingToPath = !isPreDrawingPhase && isInShortestPath;

                double tArrowTangent = 0.95;
                double tangentXCalc = 2.0 * (1.0 - tArrowTangent) * (controlX - startX) + 2.0 * tArrowTangent * (endX - controlX);
                double tangentYCalc = 2.0 * (1.0 - tArrowTangent) * (controlY - startY) + 2.0 * tArrowTangent * (endY - controlY);

                var tangentLength = Math.Sqrt(tangentXCalc * tangentXCalc + tangentYCalc * tangentYCalc);
                if (tangentLength > 1e-6)
                {
                    DrawArrow(endX, endY, tangentXCalc / tangentLength, tangentYCalc / tangentLength, detailsShouldBeColoredAccordingToPath);
                }
                else
                {
                    DrawArrow(endX, endY, unitX, unitY, detailsShouldBeColoredAccordingToPath);
                }

                double tMidLabel = 0.5;
                labelPosX = Math.Pow(1.0 - tMidLabel, 2.0) * startX +
                            2.0 * (1.0 - tMidLabel) * tMidLabel * controlX +
                            Math.Pow(tMidLabel, 2.0) * endX;
                labelPosY = Math.Pow(1.0 - tMidLabel, 2.0) * startY +
                            2.0 * (1.0 - tMidLabel) * tMidLabel * controlY +
                            Math.Pow(tMidLabel, 2.0) * endY;

                double controlVecLength = Math.Sqrt(controlPointOffsetX * controlPointOffsetX + controlPointOffsetY * controlPointOffsetY);
                if (controlVecLength > 1e-6)
                {
                    labelPosX += (controlPointOffsetX / controlVecLength) * 10.0;
                    labelPosY += (controlPointOffsetY / controlVecLength) * 10.0;
                }
            }
            else
            {
                var currentLine = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = new Avalonia.Point(startX, startY),
                    EndPoint = new Avalonia.Point(endX, endY),
                    Stroke = new SolidColorBrush(Constants.GrayColor),
                    StrokeThickness = isInShortestPath ? 2.5 : 1.5
                };
                _graphCanvas.Children.Add(currentLine);
                edgeShape = currentLine;

                bool detailsShouldBeColoredAccordingToPathForStraight = isPreDrawingPhase ? false : isInShortestPath;
                DrawArrow(endX, endY, unitX, unitY, detailsShouldBeColoredAccordingToPathForStraight);
                labelPosX = midPointX;
                labelPosY = midPointY;
            }

            bool finalDetailsColorFlag = isPreDrawingPhase ? false : isInShortestPath;
            DrawWeightLabel(labelPosX, labelPosY, weight, finalDetailsColorFlag);

            return edgeShape;
        }

        private void DrawWeightLabel(double x, double y, int weight, bool isInShortestPath)
        {
            var weightLabel = new TextBlock
            {
                Text = weight.ToString(),
                FontSize = 12.0,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = isInShortestPath ? new SolidColorBrush(Constants.RedColor) : new SolidColorBrush(Constants.BlackColor)
            };

            Canvas.SetLeft(weightLabel, x - 10.0);
            Canvas.SetTop(weightLabel, y - 8.0);
            _graphCanvas.Children.Add(weightLabel);
        }

        private void DrawArrow(double endX, double endY, double dirX, double dirY, bool isInShortestPath)
        {
            double angle = Math.Atan2(dirY, dirX);

            double arrowWingAngleOffset = Math.PI / 6.0;
            double arrowAngle1 = angle - arrowWingAngleOffset;
            double arrowAngle2 = angle + arrowWingAngleOffset;

            double arrowBaseX1 = endX - Constants.ArrowSize * Math.Cos(arrowAngle1);
            double arrowBaseY1 = endY - Constants.ArrowSize * Math.Sin(arrowAngle1);
            double arrowBaseX2 = endX - Constants.ArrowSize * Math.Cos(arrowAngle2);
            double arrowBaseY2 = endY - Constants.ArrowSize * Math.Sin(arrowAngle2);

            var arrow = new Avalonia.Controls.Shapes.Polygon
            {
                Points = new Avalonia.Points(new[] {
                new Avalonia.Point(endX, endY),
                new Avalonia.Point(arrowBaseX1, arrowBaseY1),
                new Avalonia.Point(arrowBaseX2, arrowBaseY2)
            }),
                Fill = isInShortestPath ? new SolidColorBrush(Constants.RedColor) : new SolidColorBrush(Constants.GrayColor)
            };
            _graphCanvas.Children.Add(arrow);
        }

    }
}