using GraphEditor.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GraphEditor.Windows
{
    /// <summary>
    /// Логика взаимодействия для GraphEditorWindow.xaml
    /// </summary>
    public partial class GraphEditorWindow : Window
    {
        private Graph graph = new();

        private Vertex? selectedVertex = null;
        private Edge? selectedEdge = null;

        private int vertexId = 1;
        private const double VertexRadius = 20;

        public GraphEditorWindow()
        {
            InitializeComponent();
        }

        private void GraphCanvas_RightClick(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphCanvas);
            Vertex vertex = new(vertexId++, position.X, position.Y);
            graph.AddVertex(vertex);

            DrawVertex(vertex);
            selectedVertex = null;
            selectedEdge = null;

            ClearHighlight();
        }

        private void GraphCanvas_LeftClick(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphCanvas);

            ClearHighlight();

            // Поиск ближайшей вершины (срань господня)
            Vertex? clickedVertex = graph.Vertices.FirstOrDefault(vertex => Math.Sqrt(Math.Pow(vertex.X - position.X, 2) + Math.Pow(vertex.Y - position.Y, 2)) < VertexRadius);
            // Поиск ближайшего ребра (срань господня x2)
            Edge? clickedEdge = graph.Edges.FirstOrDefault(edge => DistanceToLine(edge, position) < 5);

            // Кликнули на вершину
            if (clickedVertex != null)
            {
                // Выбираем первую вершину
                if (selectedVertex == null)
                {
                    selectedVertex = clickedVertex;
                    HighlightVertex(clickedVertex);
                }
                // Выбираем вторую вершину
                else if (selectedVertex != clickedVertex)
                {
                    Edge edge = new(selectedVertex, clickedVertex);

                    graph.AddEdge(edge);
                    DrawEdge(edge);

                    selectedVertex = null;
                }
                // Повторное нажатие на вершину
                else
                {
                    IdWindow idWindow = new(clickedVertex.Id, graph.Vertices.Select(vertex => vertex.Id));

                    if (idWindow.ShowDialog() == true)
                    {
                        clickedVertex.Id = idWindow.NewId;
                        RedrawGraph();
                    }

                    selectedVertex = null;
                }

                selectedEdge = null;
            }
            // Кликнули на ребро
            else if (clickedEdge != null)
            {
                // Первое нажатие на ребро
                if (selectedEdge != clickedEdge)
                {
                    selectedEdge = clickedEdge;
                    HighlightEdge(clickedEdge);
                }
                // Повторное нажатие на ребро
                else
                {
                    WeightWindow weightWindow = new(clickedEdge.Weight);

                    if (weightWindow.ShowDialog() == true)
                    {
                        clickedEdge.Weight = weightWindow.NewWeight;
                        RedrawGraph();
                    }

                    selectedEdge = null;
                }

                selectedVertex = null;
            }
            // Кликнули на пустоту
            else
            {
                selectedVertex = null;
                selectedEdge = null;
            }
        }

        private void GraphCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            // Если нажата кнопка Del
            if (e.Key == Key.Delete)
            {
                // Удаляем выделенную вершину
                if (selectedVertex != null)
                {
                    graph.RemoveVertex(selectedVertex);
                    selectedVertex = null;
                    RedrawGraph();
                    return;
                }

                // Удаляем выделенное ребро
                if (selectedEdge != null)
                {
                    graph.RemoveEdge(selectedEdge);
                    selectedEdge = null;
                    RedrawGraph();
                    return;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            GraphCanvas.Children.Clear();

            selectedVertex = null;
            selectedEdge = null;

            graph.Vertices.Clear();
            graph.Edges.Clear();

            vertexId = 1;
        }

        private void DrawVertex(Vertex vertex)
        {
            Ellipse ellipse = new()
            {
                Width = VertexRadius * 2,
                Height = VertexRadius * 2,
                Fill = Brushes.LightGray,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(ellipse, vertex.X - VertexRadius);
            Canvas.SetTop(ellipse, vertex.Y - VertexRadius);
            GraphCanvas.Children.Add(ellipse);

            TextBlock idTextBlock = new()
            {
                Text = vertex.Id.ToString(),
            };

            // Необходимо для корректного расчёта размеров TextBlock'ов
            idTextBlock.LayoutUpdated += (s, e) =>
            {
                // Центрирование TextBlock'а в вершине
                Canvas.SetLeft(idTextBlock, vertex.X - idTextBlock.ActualWidth / 2);
                Canvas.SetTop(idTextBlock, vertex.Y - idTextBlock.ActualHeight / 2);
            };

            GraphCanvas.Children.Add(idTextBlock);
        }

        private void HighlightVertex(Vertex vertex)
        {
            Ellipse highlightEllipse = new()
            {
                Width = VertexRadius * 2,
                Height = VertexRadius * 2,
                Stroke = Brushes.Blue,
                StrokeThickness = 3,
                Fill = Brushes.Transparent,
            };

            Canvas.SetLeft(highlightEllipse, vertex.X - VertexRadius);
            Canvas.SetTop(highlightEllipse, vertex.Y - VertexRadius);

            GraphCanvas.Children.Add(highlightEllipse);
        }

        private void DrawEdge(Edge edge)
        {
            // Вектор направления от одной вершины к другой
            Vector direction = new(edge.End.X - edge.Start.X, edge.End.Y - edge.Start.Y);
            direction.Normalize();

            // Смещение к центру вершин
            double lineOffsetX = direction.X * VertexRadius;
            double lineOffsetY = direction.Y * VertexRadius;

            Line line = new()
            {
                X1 = edge.Start.X + lineOffsetX, X2 = edge.End.X - lineOffsetX,
                Y1 = edge.Start.Y + lineOffsetY, Y2 = edge.End.Y - lineOffsetY,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            GraphCanvas.Children.Add(line);

            TextBlock weightTextBlock = new()
            {
                Text = edge.Weight.ToString(),
            };

            // Определение середины линии
            double midX = (edge.Start.X + edge.End.X) / 2;
            double midY = (edge.Start.Y + edge.End.Y) / 2;

            // Смещение в зависимости от наклона линии
            double textOffsetX = Math.Abs(edge.End.Y - edge.Start.Y) > Math.Abs(edge.End.X - edge.Start.X) ? 10 : 0;
            double textOffsetY = Math.Abs(edge.End.X - edge.Start.X) > Math.Abs(edge.End.Y - edge.Start.Y) ? 10 : -10;

            Canvas.SetLeft(weightTextBlock, midX - weightTextBlock.ActualWidth / 2 + textOffsetX);
            Canvas.SetTop(weightTextBlock, midY - weightTextBlock.ActualHeight / 2 + textOffsetY);

            GraphCanvas.Children.Add(weightTextBlock);
        }

        private void HighlightEdge(Edge edge)
        {
            Line highlightLine = new()
            {
                X1 = edge.Start.X,
                X2 = edge.End.X,
                Y1 = edge.Start.Y,
                Y2 = edge.End.Y,
                Stroke = Brushes.Red,
                StrokeThickness = 5
            };

            GraphCanvas.Children.Insert(0, highlightLine);
        }

        private void RedrawGraph()
        {
            GraphCanvas.Children.Clear();

            foreach (var vertex in graph.Vertices)
            {
                DrawVertex(vertex);
            }

            foreach (var edge in graph.Edges)
            {
                DrawEdge(edge);
            }
        }

        private double DistanceToLine(Edge edge, Point point)
        {
            // ААА ГЕОМЕТРИЯ
            double x1 = edge.Start.X, y1 = edge.Start.Y;
            double x2 = edge.End.X, y2 = edge.End.Y;
            double px = point.X, py = point.Y;

            double numerator = Math.Abs((y2 - y1) * px - (x2 - x1) * py + x2 * y1 - y2 * x1);
            double denominator = Math.Sqrt(Math.Pow(y2 - y1, 2) + Math.Pow(x2 - x1, 2));

            return numerator / denominator;
        }

        private void ClearHighlight()
        {
            foreach (UIElement child in GraphCanvas.Children.OfType<UIElement>().ToList())
            {
                if (child is Ellipse ellipse && ellipse.Stroke == Brushes.Blue)
                {
                    GraphCanvas.Children.Remove(ellipse);
                }
                else if (child is Line line && line.Stroke == Brushes.Red)
                {
                    GraphCanvas.Children.Remove(line);
                }
            }
        }
    }
}
