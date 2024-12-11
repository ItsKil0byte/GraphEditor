using GraphEditor.Elements;
using GraphEditor.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Graph graph = new();

        private Vertex? selectedVertex = null;
        private Edge? selectedEdge = null;

        private int vertexId = 1;
        private const double VertexRadius = 20;

        private Vertex? draggingVertex = null;
        private bool isDragging = false;

        private bool IsAlgorithmRunning = false;
        private CancellationTokenSource? cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GraphCanvas_RightClick(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphCanvas);

            if (!IsPointNearVertex(position))
            {
                Vertex vertex = new(vertexId++, position.X, position.Y);
                graph.AddVertex(vertex);

                DrawVertex(vertex);
            }

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

                    if (graph.AddEdge(edge))
                    {
                        DrawEdge(edge);
                    }

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
                    WeightWindow weightWindow = new(clickedEdge.Weight, clickedEdge.Capacity, clickedEdge.isDirectionShowed, clickedEdge.Direction);

                    if (weightWindow.ShowDialog() == true)
                    {
                        clickedEdge.Weight = weightWindow.NewWeight;
                        clickedEdge.Capacity = weightWindow.NewCapacity;
                        clickedEdge.isDirectionShowed = weightWindow.isDirectionShowed;
                        clickedEdge.Direction = weightWindow.NewDirection;
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

        private void GraphCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Point clickPosition = e.GetPosition(GraphCanvas);

                draggingVertex = graph.Vertices.FirstOrDefault(vertex => Math.Sqrt(Math.Pow(vertex.X - clickPosition.X, 2) + Math.Pow(vertex.Y - clickPosition.Y, 2)) < VertexRadius);

                if (draggingVertex != null)
                {
                    isDragging = true;
                    GraphCanvas.CaptureMouse();
                }
            }
        }

        private void GraphCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggingVertex != null)
            {
                Point currentPosition = e.GetPosition(GraphCanvas);

                // Обновляем координаты вершины
                draggingVertex.X = currentPosition.X;
                draggingVertex.Y = currentPosition.Y;

                RedrawGraph();
            }
        }

        private void GraphCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                draggingVertex = null;
                GraphCanvas.ReleaseMouseCapture();

                selectedVertex = null;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Комменты будут потом
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                DefaultExt = ".csv",
                FileName = "graph"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                using StreamWriter writer = new(filePath);

                writer.Write(";");
                writer.WriteLine(string.Join(";", graph.Vertices.Select(vertex => vertex.Id.ToString())));

                foreach (var firstVertex in graph.Vertices)
                {
                    List<string> row = [firstVertex.Id.ToString()];

                    foreach (var secondVertex in graph.Vertices)
                    {
                        Edge? edge = graph.Edges.FirstOrDefault(edge =>
                            (edge.Start == firstVertex && edge.End == secondVertex) ||
                            (edge.Start == secondVertex && edge.End == firstVertex));

                        row.Add(edge != null ? edge.Weight.ToString() : "0");
                    }

                    writer.WriteLine(string.Join(";", row));
                }

                writer.WriteLine();

                writer.Write(";");
                writer.WriteLine(string.Join(";", graph.Vertices.Select(vertex => vertex.Id.ToString())));

                writer.Write("X;");
                writer.WriteLine(string.Join(";", graph.Vertices.Select(vertex => vertex.X.ToString())));

                writer.Write("Y;");
                writer.WriteLine(string.Join(";", graph.Vertices.Select(vertex => vertex.Y.ToString())));
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            // Комменты будут потом
            OpenFileDialog openFileDialog = new()
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                DefaultExt = ".csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                using StreamReader reader = new(filePath);

                string? header = reader.ReadLine();
                List<int> ids = header!.Split(";").Skip(1).Select(id => int.Parse(id.Trim())).ToList();
                List<Edge> edges = [];
                List<Vertex> loadedVertices = [];

                loadedVertices.AddRange(ids.Select(id => new Vertex(id, 0, 0)));

                int row = 0;

                while (true)
                {
                    string? rowLine = reader.ReadLine();

                    if (rowLine!.Equals("")) { break; }

                    List<int> weights = rowLine!.Split(";").Skip(1).Select(weight => int.Parse(weight.Trim())).ToList();

                    for (int column = 0; column < weights.Count; column++)
                    {
                        int weight = weights[column];

                        if (weight != 0)
                        {
                            Vertex startVertex = loadedVertices[row];
                            Vertex endVertex = loadedVertices[column];

                            Edge edge = new(startVertex, endVertex)
                            {
                                Weight = weight
                            };

                            edges.Add(edge);
                        }
                    }
                    row++;
                }

                reader.ReadLine();

                string? xLine = reader.ReadLine();
                List<double> xCoordinates = xLine!.Split(";").Skip(1).Select(coordinate => double.Parse(coordinate.Trim())).ToList();

                string? yLine = reader.ReadLine();
                List<double> yCoordinates = yLine!.Split(";").Skip(1).Select(coordinate => double.Parse(coordinate.Trim())).ToList();

                for (int i = 0; i < loadedVertices.Count; i++)
                {
                    loadedVertices[i].X = xCoordinates[i];
                    loadedVertices[i].Y = yCoordinates[i];
                }

                graph.Vertices.Clear();
                graph.Edges.Clear();

                foreach (var vertex in loadedVertices)
                {
                    graph.AddVertex(vertex);
                }

                foreach (var edge in edges)
                {
                    graph.AddEdge(edge);
                }

                vertexId = loadedVertices[^1].Id + 1;

                RedrawGraph();
            }
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
                X1 = edge.Start.X + lineOffsetX,
                
                X2 = edge.End.X - lineOffsetX,
                Y1 = edge.Start.Y + lineOffsetY,
                Y2 = edge.End.Y - lineOffsetY,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            GraphCanvas.Children.Add(line);

            if (edge.isDirectionShowed)
            {
                // Координаты конца или начала стрелки в зависимости от edge.Direction
                double arrowX, arrowY;
                double baseX, baseY;

                if (edge.Direction)
                {
                    arrowX = line.X1;
                    arrowY = line.Y1;
                    baseX = line.X2;
                    baseY = line.Y2;
                }
                else
                {
                    arrowX = line.X2;
                    arrowY = line.Y2;
                    baseX = line.X1;
                    baseY = line.Y1;
                }

                // Углы для построения треугольной стрелки
                double angle = Math.Atan2(baseY - arrowY, baseX - arrowX);
                double arrowLength = 10; // Длина стрелки
                double arrowWidth = 5;   // Ширина стрелки

                Point arrowTip = new Point(arrowX, arrowY);
                Point arrowLeft = new Point(
                    arrowX + arrowLength * Math.Cos(angle + Math.PI / 6),
                    arrowY + arrowLength * Math.Sin(angle + Math.PI / 6)
                );
                Point arrowRight = new Point(
                    arrowX + arrowLength * Math.Cos(angle - Math.PI / 6),
                    arrowY + arrowLength * Math.Sin(angle - Math.PI / 6)
                );

                // Создаем треугольник для стрелки
                Polygon arrow = new Polygon
                {
                    Points = new PointCollection { arrowTip, arrowLeft, arrowRight },
                    Fill = Brushes.Black
                };

                // Добавляем стрелку в Canvas
                GraphCanvas.Children.Add(arrow);
            }

            TextBlock weightTextBlock = new()
            {
                Text = edge.Weight.ToString() + (edge.Capacity != 0 ? "/" + edge.Capacity.ToString() : string.Empty)
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
            // ААА, ЕЩЁ БОЛЕЕ СТРАШНАЯ ГЕОМЕТРИЯ
            // Не лезть, опасно для жизни
            double x1 = edge.Start.X, y1 = edge.Start.Y;
            double x2 = edge.End.X, y2 = edge.End.Y;
            double px = point.X, py = point.Y;

            // Вычисление вектора
            double dx = x2 - x1, dy = y2 - y1;

            // Длина вектора
            double len = dx * dx + dy * dy;

            // Проекция точки на прямую
            double projection = Math.Max(0, Math.Min(1, ((px - x1) * dx + (py - y1) * dy) / len));

            // Вычисление ближайшей точки
            double projectionX = x1 + projection * dx;
            double projectionY = y1 + projection * dy;

            // Расстоняние от точки до ближайшей точки на отрезке
            return Math.Sqrt(Math.Pow(px - projectionX, 2) + Math.Pow(py - projectionY, 2));
        }

        private bool IsPointNearVertex(Point point)
        {
            return graph.Vertices.Any(vertex => Math.Sqrt(Math.Pow(vertex.X - point.X, 2) + Math.Pow(vertex.Y - point.Y, 2)) < VertexRadius * 2);
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

        private async Task DepthFirstSearchAsync(Vertex startVertex)
        {
            HashSet<Vertex> visited = [];
            Stack<Vertex> stack = [];
            stack.Push(startVertex);

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            StepsTextBox.Clear();
            StepsTextBox.AppendText("Алгоритм: Обход в глубину (DFS)\n");
            StepsTextBox.AppendText("Идея: Двигаемся как можно глубже по рёбрам, возвращаясь назад, если больше нет вариантов.\n\n");

            await Task.Delay(1500);

            while (stack.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StepsTextBox.AppendText("Процесс остановлен пользователем.\n");
                    return;
                }

                Vertex currentVertex = stack.Pop();
                if (!visited.Contains(currentVertex))
                {
                    visited.Add(currentVertex);
                    HighlightVertex(currentVertex); // Подсветить вершину
                    StepsTextBox.AppendText($"Посещена вершина: {currentVertex.Id}\n");
                    await Task.Delay(1500); // Задержка

                    foreach (var edge in graph.Edges.Where(e => e.Start == currentVertex || e.End == currentVertex))
                    {
                        Vertex neighbor = edge.Start == currentVertex ? edge.End : edge.Start;
                        if (!visited.Contains(neighbor))
                        {
                            stack.Push(neighbor);
                            HighlightEdge(edge); // Подсветить ребро
                            StepsTextBox.AppendText($"Переходим по рёбру от вершины {currentVertex.Id} к вершине {neighbor.Id}.\n");
                            await Task.Delay(1500); // Задержка
                        }
                        else
                        {
                            StepsTextBox.AppendText($"Вершина {currentVertex.Id} уже посещена. Возвращаемся назад.\n");
                        }
                    }
                }
            }

            StepsTextBox.AppendText("\nОбход в глубину завершён: все вершины, до которых можно добраться, посещены.\n");
        }

        private async Task BreadthFirstSearchAsync(Vertex startVertex)
        {
            HashSet<Vertex> visited = [];
            Queue<Vertex> queue = [];
            queue.Enqueue(startVertex);

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            StepsTextBox.Clear();
            StepsTextBox.AppendText("Алгоритм: Обход в ширину (BFS)\n");
            StepsTextBox.AppendText("Идея: Исследуем вершины слоями — сначала ближайших соседей, затем их соседей и так далее.\n\n");

            await Task.Delay(1500);

            while (queue.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StepsTextBox.AppendText("Процесс остановлен пользователем.\n");
                    return;
                }

                Vertex currentVertex = queue.Dequeue();
                if (!visited.Contains(currentVertex))
                {
                    visited.Add(currentVertex);
                    HighlightVertex(currentVertex); // Подсветить вершину
                    StepsTextBox.AppendText($"Посещена вершина: {currentVertex.Id}\n");
                    await Task.Delay(1500); // Задержка

                    foreach (var edge in graph.Edges.Where(e => e.Start == currentVertex || e.End == currentVertex))
                    {
                        Vertex neighbor = edge.Start == currentVertex ? edge.End : edge.Start;
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            HighlightEdge(edge); // Подсветить ребро
                            StepsTextBox.AppendText($"Добавляем в очередь вершину {neighbor.Id}, соседнюю с вершиной {currentVertex.Id}.\n");
                            await Task.Delay(1500); // Задержка
                        }
                        else
                        {
                            StepsTextBox.AppendText($"Вершина {currentVertex.Id} уже посещена.\n");
                        }
                    }
                }
            }

            StepsTextBox.AppendText("\nОбход в ширину завершён: все вершины, до которых можно добраться, посещены.\n");
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAlgorithmRunning)
            {
                cancellationTokenSource?.Cancel();
                IsAlgorithmRunning = false;
                StartButton.Content = "Запустить";
                UnblockUIElements();
                return;
            }

            IsAlgorithmRunning = true;
            StartButton.Content = "Остановить";
            BlockUIElements();

            string? selectedAlgorithm = (AlgSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            if (selectedAlgorithm == "Поиск кратчайшего пути")
            {
                VertexSelectionDialog dialog = new();
                if (dialog.ShowDialog() == true)
                {
                    int? startId = dialog.StartVertexId;
                    int? endId = dialog.EndVertexId;

                    Vertex startVertex = graph.Vertices.FirstOrDefault(v => v.Id == startId);
                    Vertex endVertex = graph.Vertices.FirstOrDefault(v => v.Id == endId);

                    if (startVertex != null && endVertex != null)
                    {
                        ClearGreenHighlight();
                        await DijkstraAsync(startVertex, endVertex);
                    }
                    else
                    {
                        MessageBox.Show("Одна или обе вершины не найдены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            
            else if (selectedAlgorithm == "Минимальное остовное дерево")
            {
                ClearGreenHighlight();
                await MinimumSpanningTreeAsync();
            }
            
            else if (selectedAlgorithm == "Обход в глубину")
            {
                if (selectedVertex != null)
                {
                    ClearGreenHighlight();
                    await DepthFirstSearchAsync(selectedVertex);
                }
                else
                {
                    MessageBox.Show(
                        "Перед началом обхода необходимо выбрать вершину", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning
                    );
                }
            }
            else if (selectedAlgorithm == "Обход в ширину")
            {
                if (selectedVertex != null)
                {
                    ClearGreenHighlight();
                    await BreadthFirstSearchAsync(selectedVertex);
                }
                else
                {
                    MessageBox.Show(
                        "Перед началом обхода необходимо выбрать вершину", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning
                    );
                }
            }

            IsAlgorithmRunning = false;
            StartButton.Content = "Запустить";
            selectedVertex = null;
            UnblockUIElements();

            ClearHighlight();
        }

        private void BlockUIElements()
        {
            ToolBox.IsEnabled = false;
            GraphCanvas.IsEnabled = false;
            AlgSelector.IsEnabled = false;
        }

        private void UnblockUIElements()
        {
            ToolBox.IsEnabled = true;
            GraphCanvas.IsEnabled = true;
            AlgSelector.IsEnabled = true;
        }

        private void StepsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            StepsTextBox.ScrollToEnd();
        }
        
        private async Task DijkstraAsync(Vertex startVertex, Vertex endVertex)
        {
            Dictionary<Vertex, int> distances = graph.Vertices.ToDictionary(v => v, v => int.MaxValue);
            distances[startVertex] = 0;

            HashSet<Vertex> visited = new();
            PriorityQueue<Vertex, int> priorityQueue = new();
            priorityQueue.Enqueue(startVertex, 0);

            StepsTextBox.Clear();
            StepsTextBox.AppendText($"Начинаем поиск кратчайшего пути между вершинами {startVertex.Id} и {endVertex.Id}.\n");
            StepsTextBox.AppendText("Инициализируем расстояния до всех вершин как бесконечность.\n");
            StepsTextBox.AppendText($"Расстояние до вершины {startVertex.Id} установлено на 0.\n");

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            while (priorityQueue.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StepsTextBox.AppendText("\nПроцесс остановлен пользователем.\n");
                    return;
                }

                Vertex currentVertex = priorityQueue.Dequeue();
                visited.Add(currentVertex);

                HighlightVertex(currentVertex);
                StepsTextBox.AppendText($"\nВыбрана вершина {currentVertex.Id} с весом {distances[currentVertex]}.\n");
                await Task.Delay(2500);

                foreach (var edge in graph.Edges.Where(e => e.Start == currentVertex || e.End == currentVertex))
                {
                    Vertex neighbor = edge.Start == currentVertex ? edge.End : edge.Start;

                    if (visited.Contains(neighbor)) continue;

                    int newDist = distances[currentVertex] + edge.Weight;

                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        priorityQueue.Enqueue(neighbor, newDist);
                        HighlightEdge(edge);
                        StepsTextBox.AppendText($"Обновлен вес вершины {neighbor.Id} до {newDist} (через вершину {currentVertex.Id}).\n");
                        await Task.Delay(2500);
                    }
                    else
                    {
                        StepsTextBox.AppendText($"Вес вершины {neighbor.Id} не обновлен (текущий вес: {distances[neighbor]}, а новый вес: {newDist}, что нам не подходит).\n");
                        await Task.Delay(2500);
                    }
                }
            }

            Vertex pathVertex = endVertex;
            List<Edge> pathEdges = new();

            StepsTextBox.AppendText($"\nВосстанавливаем путь от вершины {endVertex.Id} к вершине {startVertex.Id}.\n");
            StepsTextBox.AppendText("\n");
            ClearHighlight();
            while (pathVertex != startVertex)
            {
                bool found = false;
                foreach (var edge in graph.Edges.Where(e => e.Start == pathVertex || e.End == pathVertex))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        StepsTextBox.AppendText("Процесс остановлен пользователем.\n");
                        return;
                    }

                    Vertex neighbor = edge.Start == pathVertex ? edge.End : edge.Start;
                    if (distances[neighbor] + edge.Weight == distances[pathVertex])
                    {
                        pathEdges.Add(edge);
                        pathVertex = neighbor;
                        found = true;
                        StepsTextBox.AppendText($"Переход к вершине {neighbor.Id} через ребро с весом {edge.Weight}.\n");
                        HighlightShortestPathEdge(edge); // Вызываем новый метод для подсветки ребра
                        await Task.Delay(5500);
                        break;
                    }
                }
                if (!found)
                {
                    StepsTextBox.AppendText($"Путь не найден от вершины {pathVertex.Id}.\n");
                    break;
                }
            }

            foreach (var edge in pathEdges)
            {
                HighlightEdge(edge);
            }

            StepsTextBox.AppendText($"\nМинимальный путь найден! Длина пути: {distances[endVertex]}.\n");
        }
        private void HighlightShortestPathEdge(Edge edge)
        //чуть-чуть наговнокодил, ибо можно было методы Санька чуть переписать)
        {
            Line highlightLine = new()
            {
                X1 = edge.Start.X,
                X2 = edge.End.X,
                Y1 = edge.Start.Y,
                Y2 = edge.End.Y,
                Stroke = Brushes.Green,
                StrokeThickness = 5
            };

            GraphCanvas.Children.Insert(0, highlightLine);
        }
        
        private void ClearGreenHighlight() 
        //чуть-чуть наговнокодил, ибо можно было методы Санька чуть переписать)
        {
            foreach (UIElement child in GraphCanvas.Children.OfType<UIElement>().ToList())
            {
                if (child is Line greenLine && greenLine.Stroke == Brushes.Green)
                {
                    GraphCanvas.Children.Remove(greenLine);
                }
            }
        }
        
        private async Task MinimumSpanningTreeAsync()
        {
            var edgesWithId = graph.Edges.Select((edge, index) => new EdgeWithId(edge.Start, edge.End, index + 1) { Weight = edge.Weight }).ToList();
            var sortedEdges = edgesWithId.OrderBy(e => e.Weight).ToList();

            Dictionary<Vertex, Vertex> parent = new();
            HashSet<Vertex> visited = new();
            StepsTextBox.Clear();
            StepsTextBox.AppendText("Алгоритм: Поиск минимального остовного дерева (Краскал)\n");

            foreach (var vertex in graph.Vertices)
            {
                parent[vertex] = vertex; 
            }

            Vertex Find(Vertex vertex)
            {
                if (parent[vertex] != vertex)
                {
                    parent[vertex] = Find(parent[vertex]);
                }
                return parent[vertex];
            }

            async Task HighlightTemporarily(EdgeWithId edge)
            {
                HighlightChecker(edge, Brushes.Blue);
                await Task.Delay(2000); 
            }

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            
            foreach (var edge in sortedEdges)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StepsTextBox.AppendText("\nПроцесс остановлен пользователем.\n");
                    return;
                }

                Vertex root1 = Find(edge.Start);
                Vertex root2 = Find(edge.End);

                StepsTextBox.AppendText($"\nПроверяем ребро между вершинами №{edge.Start.Id} и №{edge.End.Id} с весом {edge.Weight}.\n");
                await HighlightTemporarily(edge);
                
                if (root1 != root2)
                {
                    visited.Add(edge.Start);
                    visited.Add(edge.End);
                    parent[root1] = root2;
                    
                    HighlightShortestPathEdge(edge); 
                    StepsTextBox.AppendText($"Добавляем ребро между вершинами №{edge.Start.Id} и №{edge.End.Id} в остовное дерево.\n");
                    await Task.Delay(2500);
                }
                else
                {
                    StepsTextBox.AppendText($"Пропускаем ребро между вершинами №{edge.Start.Id} и №{edge.End.Id}, так как оно создает цикл.\n");
                }
            }

            StepsTextBox.AppendText("\nМинимальное остовное дерево построено.\n");
        }

        
        private void HighlightChecker(Edge edge, Brush color)
        //ещё чуть чуть наговнокодил)))))))))
        {
            Line highlightLine = new()
            {
                X1 = edge.Start.X,
                X2 = edge.End.X,
                Y1 = edge.Start.Y,
                Y2 = edge.End.Y,
                Stroke = color,
                StrokeThickness = 5
            };

            GraphCanvas.Children.Insert(0, highlightLine);
            
            if (color == Brushes.Blue)
            {
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GraphCanvas.Children.Remove(highlightLine);
                    });
                });
            }
        }
    }
}