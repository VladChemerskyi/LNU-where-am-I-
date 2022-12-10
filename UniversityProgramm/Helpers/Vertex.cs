using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UniversityProgramm.Helpers
{
    /// <summary>
    /// Definition of Vertex
    /// </summary>
    public class Vertex : IEqualityComparer<Vertex>
    {
        public string Name { get; set; }

        public Point Position { get; set; }

        /// <summary>
        /// First is vertex, second is lenght to current vertex
        /// </summary>
        public List<Pair<Vertex, double>> Neibours { get; private set; }
        public static string LastName { get; private set; }

        static Vertex()
        {
            LastName = "0";
        }
        public Vertex()
        {
            Name = "Empty";
            Neibours = new List<Pair<Vertex, double>>();
        }

        public Vertex(string name): this()
        {
            Name = name;
        }

        public Vertex(string name, params Vertex[] vertices)
        {
            Name = name;
            Add(vertices);
        }

        public Vertex(string name, params Pair<Vertex, double>[] vertices)
        {
            Name = name;
            Add(vertices);
        }

        public void Add(params Vertex[] vertices)
        {
            foreach (var item in vertices)
            {
                Pair<Vertex, double> pair = new Pair<Vertex, double>(item, 0);

                Neibours.Add(pair);
            }
        }

        public void Add(params Pair<Vertex, double>[] vertices)
        {
            foreach (var item in vertices)
            {
                Neibours.Add(item);
            }
        }

        public double GetVertexLengthFromName(string name)
        {
            for (int i = 0; i < Neibours.Count(); i++)
            {
                if (Neibours[i].First.Name == name)
                {
                    return Neibours[i].Second;
                }
            }

            return -1;
        }

        public static string GetNextName()
        {
            long currentName = Convert.ToInt64(LastName);

            currentName++;
            LastName = currentName.ToString();

            return LastName;
        }

        public bool Equals(Vertex x, Vertex y)
        {
            if (x.Name == y.Name)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(Vertex obj)
        {
            return this.GetHashCode();
        }
    }
}
