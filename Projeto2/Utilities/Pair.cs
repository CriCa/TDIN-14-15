using System;

namespace Utilities
{
    [Serializable]
    public class Pair<T, V>
    {
        T First;
        V Second;

        public T first { get { return this.First; } set { this.First = value; } }
        public V second { get { return this.Second; } set { this.Second = value; } }

        public Pair(T f, V s)
        {
            this.first = f;
            this.second = s;
        }
    }
}
