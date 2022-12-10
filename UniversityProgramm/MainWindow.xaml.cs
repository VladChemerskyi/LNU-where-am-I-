using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Collections.Generic;
using UniversityProgramm.Helpers;
using System.Windows.Data;
using UniversityProgramm.ViewModels;
using System.Linq;
using OpenTK.Audio.OpenAL;

namespace UniversityProgramm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double MaximumHeight { get => Height; }
        public double MaximumWidth { get => Width; }
        public MainWindowModel CurrentDataContext { get => DataContext as MainWindowModel; }
        public double MapHeight 
        { 
            get => CurrentDataContext.MapHeight; 
            set => CurrentDataContext.MapHeight = value; 
        }
        public double MapWidth 
        { 
            get => CurrentDataContext.MapWidth; 
            set => CurrentDataContext.MapWidth = value; 
        }
        private int _currentLevel;
        public int CurrentLevel { 
            get => _currentLevel;
            set 
            {
                if (value >= 0 && value <= 3)
                {
                    _currentLevel = value;
                }
            }
        }

        private int _delta = 120;
        private float _persantage = 0.1f;
        private float _maxPersantage = 2f;
        private float _currentPersantage = 1f;
        private double _normalHeight = 960;
        private double _normalWidth = 1280;
        private Image _map;
        private List<Line> _lines;
        private Graph _graph;
        private Point _totalOffset;
        private string _picturePath = $"pack://application:,,,/Images/MainCorps/1.~.png";

        /// <summary>
        /// Set First floor as map
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Graph graph = GraphBuilder.BuildGraphFromVersicesFile("../../Resourses/Path.txt");

            var names = from i in graph.Vertices
                        where i.Name[0] != 'A' && i.Name[0] != 'S'
                        orderby i.Name
                        select i.Name;

            From.Items.Add(names.Last());
            To.Items.Add(names.Last());

            foreach (var item in names)
            {
                if (!(item == "Вхід")) {
                    From.Items.Add(item);
                    To.Items.Add(item); 
                }
            }

            _lines = new List<Line>();
            _graph = graph;
            _totalOffset = new Point(0, 0);
            _currentLevel = 1;
            string picturePath = _picturePath.Replace('~', CurrentLevel.ToString()[0]);
            AddPicture(picturePath);
        }

        /// <summary>
        /// Exit from application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Draw Line from firstPoint to secondPoint
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        public void DrawLine(Point firstPoint, Point secondPoint, string name)
        {
            Line line = new Line
            {
                X1 = firstPoint.X + _totalOffset.X,
                X2 = secondPoint.X + _totalOffset.X,
                Y1 = firstPoint.Y + _totalOffset.Y,
                Y2 = secondPoint.Y + _totalOffset.Y,
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Name = name,
                
            };
            _lines.Add(line);
            Path.Children.Add(line);
        }

        /// <summary>
        /// Delete all lines
        /// </summary>
        /// <returns></returns>
        public bool ClearAllLines()
        {
            List<Line> lines = new List<Line>();

            foreach (var item in Path.Children)
            {
                if(item is Line)
                {
                    lines.Add(item as Line);
                }
            }

            foreach (var item in lines)
            {
                Path.Children.Remove(item);
            }
            _lines.Clear();
            return true;
        }

        /// <summary>
        /// Set picture as map
        /// </summary>
        /// <param name="path"></param>
        private void AddPicture(string path)
        {
            var bitmap = new BitmapImage(new Uri(path));

            MapHeight = bitmap.PixelHeight;
            MapWidth = bitmap.PixelWidth;

            _draggedImage = new Image() { Source = bitmap };

            _draggedImage.Name = "MapPicture";

            _normalHeight = bitmap.Height;
            _normalWidth = bitmap.Width;

            Canvas.SetLeft(_draggedImage, 0);
            Canvas.SetTop(_draggedImage, 0);

            foreach (Image item in canvas.Children)
            {
                if (item != null && item.Name == "MapPicture")
                {
                    canvas.Children.Remove(item);
                    break;
                }
            }

            ShowCurrentPath();

            canvas.Children.Add(_draggedImage);
            SetBindings(_draggedImage);

            Canvas.SetLeft(_draggedImage, Canvas.GetLeft(_draggedImage) + _totalOffset.X);
            Canvas.SetTop(_draggedImage, Canvas.GetTop(_draggedImage) + _totalOffset.Y);
        }

        private void ShowCurrentPath()
        {
            foreach (Line line in Path.Children)
            {
                string[] toSplit = new string[]
                {
                    "ToSplit"
                };

                List<string> names = line.Name.Split(toSplit, StringSplitOptions.None).ToList();
                bool condition =
                    names.Contains("STAIRS") ||
                    names.Select(x => x[0]).Contains(CurrentLevel.ToString()[0]) ||
                    (names.Select(x => x[0]).Contains('A') && names.Select(x => x[1]).Contains(CurrentLevel.ToString()[0]));
                if (condition)
                {
                    line.Visibility = Visibility.Visible;
                }
                else
                {
                    line.Visibility = Visibility.Hidden;
                }
            }
        }

        /// <summary>
        /// Bind Height and width for image
        /// </summary>
        /// <param name="image"></param>
        private void SetBindings(Image image)
        {
            Binding heightBinding = new Binding
            {
                Source = (DataContext as MainWindowModel),
                Path = new PropertyPath("MapHeight"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            Binding widthBinding = new Binding
            {
                Source = (DataContext as MainWindowModel),
                Path = new PropertyPath("MapWidth"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(image, Image.HeightProperty, heightBinding);
            BindingOperations.SetBinding(image, Image.WidthProperty, widthBinding);
        }

        private Image _draggedImage;
        private Point _mousePosition;
        private bool _isDragged = false;

        /// <summary>
        /// Let map move if mouse button down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = _draggedImage;//e.Source as Image;

            if (image != null && canvas.CaptureMouse())
            {
                Mouse.OverrideCursor = Cursors.ScrollAll;
                _mousePosition = e.GetPosition(canvas);
                _draggedImage = image;
                //Panel.SetZIndex(_draggedImage, 1);

                _isDragged = true;
            }
        }

        /// <summary>
        /// Do not move map if mouse button up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_draggedImage != null)
            {
                _isDragged = false;

                Mouse.OverrideCursor = Cursors.Arrow;
                canvas.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Move map with mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedImage != null && _isDragged)
            {
                var position = e.GetPosition(canvas);
                var offset = position - _mousePosition;
                _mousePosition = position;

                Point relativePoint = _draggedImage.TransformToAncestor(Map).Transform(new Point(0, 0));

                double toX = relativePoint.X + offset.X;
                double toY = relativePoint.Y + offset.Y;

                if ((offset.X > 0 && toX <= 0) || (offset.X < 0 && -toX + Map.ActualWidth <= _draggedImage.ActualWidth))
                {
                    Canvas.SetLeft(_draggedImage, Canvas.GetLeft(_draggedImage) + offset.X);
                    _totalOffset.X += offset.X;
                    int i = 0;
                    foreach (Line item in Path.Children)
                    {
                        item.X1 += offset.X;
                        item.X2 += offset.X;
                        _lines[i] = item;
                    }
                }

                if ((offset.Y > 0 && toY <= 0) || (offset.Y < 0 && -toY + Map.ActualHeight <= _draggedImage.ActualHeight))
                {
                    Canvas.SetTop(_draggedImage, Canvas.GetTop(_draggedImage) + offset.Y);
                    _totalOffset.Y += offset.Y;
                    int i = 0;
                    foreach (Line item in Path.Children)
                    {
                        item.Y1 += offset.Y;
                        item.Y2 += offset.Y;
                        _lines[i] = item;
                    }
                }
            }
        }

        /// <summary>
        /// Open Find UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Find(object sender, RoutedEventArgs e)
        {
            SwapVisibilities();
            ClearAllLines();
        }

        /// <summary>
        /// Change visibilty in Find UI
        /// </summary>
        private void SwapVisibilities()
        {
            if (From.Visibility == Visibility.Collapsed)
            {
                From.Visibility = Visibility.Visible;
                To.Visibility = Visibility.Visible;
                Search.Visibility = Visibility.Visible;
            }
            else
            {
                From.Visibility = Visibility.Collapsed;
                To.Visibility = Visibility.Collapsed;
                Search.Visibility = Visibility.Collapsed;
            }
        }

        private void ToMap(object sender, RoutedEventArgs e)
        {
            ClearAllLines();
        }

        /// <summary>
        /// Resize map with mouse Wheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (_map == null)
            //{
            //    foreach (var item in canvas.Children)
            //    {
            //        Image image = item as Image;
            //        float newPersantage = ((e.Delta / _delta) * _persantage + _currentPersantage);
            //        newPersantage = (newPersantage <= _maxPersantage ? newPersantage : _maxPersantage);

            //        double newPersantageHeight = newPersantage * _normalHeight;
            //        double newPersantageWidth = newPersantage * _normalWidth;

            //        bool isNormalHeight = newPersantageHeight >= Map.ActualHeight;
            //        bool isNormalWidth = newPersantageHeight >= Map.ActualWidth;
            //        bool isNormalImage = image != null && image.Name == "MapPicture";

            //        if (isNormalImage && (isNormalHeight || isNormalWidth))
            //        {
            //            _currentPersantage = newPersantage;

            //            if (_normalHeight < _normalWidth)
            //            {
            //                CurrentDataContext.MapHeight = newPersantageHeight;
            //            }
            //            else
            //            {
            //                CurrentDataContext.MapWidth = newPersantageWidth;
            //            }

            //            break;
            //        }
            //    }
            //    for(int i = 0; i < Path.Children.Count; i++)
            //    {
            //        float newPersantage = ((e.Delta / _delta) * _persantage + _currentPersantage);
            //        newPersantage = (newPersantage <= _maxPersantage ? newPersantage : _maxPersantage);

            //        (Path.Children[i] as Line).X1 = _lines[i].X1 * newPersantage;
            //        (Path.Children[i] as Line).X2 = _lines[i].X2 * newPersantage;
            //        (Path.Children[i] as Line).Y1 = _lines[i].Y1 * newPersantage;
            //        (Path.Children[i] as Line).Y2 = _lines[i].Y2 * newPersantage;
            //    }
            //}
        }

        /// <summary>
        /// Build Path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ClearAllLines();
            try
            {
                if (From.SelectedItem != null && To.SelectedItem != null)
                {
                    string from = From.SelectedItem as string;
                    string to = To.SelectedItem as string;

                    var matrix = _graph.ToMatrix();

                    DijkstrasAlgorithm dijkstras = new DijkstrasAlgorithm();

                    var path = dijkstras.Dijkstra(matrix, _graph.GetVertexPositionByName(from), _graph.GetVertexPositionByName(to));

                    for (int i = 0; i < path.Count; i++)
                    {
                        Vertex vertex = _graph.Vertices[path[i]];
                        foreach (var item2 in vertex.Neibours)
                        {
                            DrawLine(vertex.Position, item2.First.Position, vertex.Name + "ToSplit" + item2.First.Name);
                        }
                    }
                }
                ShowCurrentPath();
            }
            catch (Exception ex)
            {

            }
        }

        private void Button_Up(object sender, RoutedEventArgs e)
        {
            if (CurrentLevel != 3)
            {
                CurrentLevel++;
                string picturePath = _picturePath.Replace('~', CurrentLevel.ToString()[0]);
                AddPicture(picturePath);
            }
        }

        private void Button_Down(object sender, RoutedEventArgs e)
        {
            if (CurrentLevel != 1)
            {
                CurrentLevel--;
                string picturePath = _picturePath.Replace('~', CurrentLevel.ToString()[0]);
                AddPicture(picturePath);
            }
        }
    }
}
