using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityProgramm.Helpers
{
    public class Graphs
    {
        public List<Graph> Piramid { get; set; }

        public Graphs(params Graph[] graphs)
        {
            Piramid = new List<Graph>();

            foreach (var item in graphs)
            {
                Piramid.Add(item);
            }
        }
    }
}
