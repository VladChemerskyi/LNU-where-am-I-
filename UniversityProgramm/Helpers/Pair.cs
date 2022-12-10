using System.Drawing;

namespace UniversityProgramm.Helpers
{
    /// <summary>
    /// Just class for represent pair
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class Pair<T,V>
    {
        public T First { get; set; }
        public V Second { get; set; }

        public Pair()
        {

        }

        public Pair(T first, V second)
        {
            First = first;
            Second = second;
        }

        public Pair(Pair<T, V> otherPair)
        {
            CopyFrom(otherPair);
        }

        public void CopyFrom(Pair<T, V> otherPair)
        {
            First = otherPair.First;
            Second = otherPair.Second;
        }
    }
}
