using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Drawing.Image;

namespace UniversityProgramm.Helpers
{
    /// <summary>
    /// Graph builder
    /// </summary>
    public class GraphBuilder
    {
        public static Color PathColor { get; private set; } = Color.FromArgb(255, 98, 178, 246);
        public static Color NodeColor { get; private set; } = Color.FromArgb(255, 222, 141, 98);
        public static Color DownFloorColor { get; private set; } = Color.FromArgb(255, 98, 0, 0);
        public static Color UpFloorColor { get; private set; } = Color.FromArgb(255, 98, 145, 98);

        private static List<Color> _colorsOfVertix;

        static GraphBuilder()
        {
            _colorsOfVertix = new List<Color>()
            {
                NodeColor,
                DownFloorColor,
                UpFloorColor
            };
        }

        public static double[,] BuildFromFile(Uri fileUri)
        {
            BitmapImage pathBitMap = new BitmapImage(fileUri);
            Bitmap pathMap = BitmapImage2Bitmap(pathBitMap);

            Pair<int, int> firstVertex = FindFirstVertex(pathMap);

            

            return new double[0, 0];
        }

        private static List<Pair<int, int>> GetBeginsPathesFromVertex(Pair<int, int> vertex, Bitmap pathMap)
        {
            List<Pair<int, int>> pathes = new List<Pair<int, int>>();

            for (int x = 0; x < vertex.First + 3 && x < pathMap.Width; x++)
            {
                for (int y = 0; y < vertex.Second + 3 && y < pathMap.Height; y++)
                {
                    Color currentPixelColor = pathMap.GetPixel(x, y);

                    if (currentPixelColor == PathColor)
                    {
                        pathes.Add(new Pair<int, int>(x, y));
                    }
                }
            }

            return pathes;
        }

        private static Pair<int, int> GetVertixFromPath(Pair<int, int> begin, Pair<int, int> next, Bitmap pathMap)
        {
            Color currentPixelColor = new Color();
            while (!_colorsOfVertix.Contains(currentPixelColor))
            {
                for (int x = 0; x < next.First + 3 && x < pathMap.Width; x++)
                {
                    bool needToBreak = false;
                    for (int y = 0; y < next.Second + 3 && y < pathMap.Height; y++)
                    {
                        currentPixelColor = pathMap.GetPixel(x, y);

                        if (currentPixelColor == PathColor && x != begin.First && y != begin.Second)
                        {
                            needToBreak = true;

                            next = begin;
                            begin = new Pair<int, int>(x, y);

                            break;
                        }
                    }

                    if (needToBreak)
                    {
                        break;
                    }
                }
            }
            return new Pair<int, int>();
        }

        private static Pair<int, int> FindFirstVertex(Bitmap pathMap)
        {
            for (int x = 0; x < pathMap.Width; x++)
            {
                for (int y = 0; y < pathMap.Height; y++)
                {
                    Color currentPixelColor = pathMap.GetPixel(x, y);
                    if (_colorsOfVertix.Contains(currentPixelColor))
                    {
                        Pair<int, int> firstVertexPoint = new Pair<int, int>(x, y);

                        return firstVertexPoint;
                    }
                }
            }

            return new Pair<int, int>(0, 0);
        }

        private static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private static double GetLength(Pair<double, double> first, Pair<double, double> second)
        {
            double result = Math.Sqrt(Math.Pow(first.First - second.First, 2) + Math.Pow(first.Second - second.Second, 2));

            return result;
        }

        public static Graph BuildGraphFromVersicesFile(string fileUri)
        {
            string[] vertices = File.ReadAllLines(fileUri);

            Graph graph = new Graph("Graph from vertices file");

            for(int i = 0; i < vertices.Length; i++)
            {
                Pair<Vertex, Vertex> verticesPair = ParseVertexFromString(vertices[i]);

                if (!graph.Vertices.Contains(verticesPair.First))
                {
                    graph.Vertices.Add(verticesPair.First);
                }
                int firstVertexIndex = graph.Vertices.IndexOf(verticesPair.First);

                double length = GetLength(
                    new Pair<double, double>(verticesPair.First.Position.X, verticesPair.First.Position.Y),
                    new Pair<double, double>(verticesPair.Second.Position.X, verticesPair.Second.Position.Y));

                graph.Vertices[firstVertexIndex].Neibours.Add(
                    new Pair<Vertex, double>(verticesPair.Second,length)
                    );

                if (!graph.Vertices.Contains(verticesPair.Second))
                {
                    graph.Vertices.Add(verticesPair.Second);
                }
                int secondVertexIndex = graph.Vertices.IndexOf(verticesPair.Second);

                graph.Vertices[firstVertexIndex].Neibours.Add(
                    new Pair<Vertex, double>(verticesPair.First, length)
                    );
            }

            return graph;
        }

        private static Pair<Vertex, Vertex> ParseVertexFromString(string vertex)
        {
            Pair<Vertex, int> firstVertexPair = GetParsedVertex(vertex, 0);
            Pair<Vertex, int> secondVertexPair = GetParsedVertex(vertex, firstVertexPair.Second + 1);

            return new Pair<Vertex, Vertex>(firstVertexPair.First, secondVertexPair.First);
        }

        private static Pair<Vertex, int> GetParsedVertex(string vertex, int startIndex)
        {
            Pair<string, int> vertexName = GetString(vertex, startIndex);
            Pair<string, int> vertexPositionString = GetString(vertex, vertexName.Second + 1);

            string xString = vertexPositionString.First.Substring(0, vertexPositionString.First.IndexOf(','));
            int x = Convert.ToInt32(xString);
            string yString = vertexPositionString.First.Substring(
                vertexPositionString.First.IndexOf(',') + 1, 
                vertexPositionString.First.Length - vertexPositionString.First.IndexOf(',') - 1);

            int y = Convert.ToInt32(yString);
            System.Windows.Point point = new System.Windows.Point(x, y);

            Vertex firstVertex = new Vertex(vertexName.First)
            {
                Position = point
            };

            return new Pair<Vertex, int>(firstVertex, vertexPositionString.Second);
        }

        private static Pair<string, int> GetString(string vertex, int startIndex)
        {
            int firstRoundRightIndex = vertex.IndexOf('(', startIndex);
            int firstRoundLeftIndex = vertex.IndexOf(')', startIndex);

            string result = vertex.Substring(firstRoundRightIndex + 1, firstRoundLeftIndex - firstRoundRightIndex - 1);

            return new Pair<string, int>(result, firstRoundLeftIndex);
        }
    }
}
