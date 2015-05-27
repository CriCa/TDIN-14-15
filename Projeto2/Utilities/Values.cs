using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public class Values
    {
        Dictionary<string, Pair<Type, object>> values = new Dictionary<string, Pair<Type, object>>();

        public int size { get { return values.Keys.Count; } }

        public List<string> keys { get { return values.Keys.ToList(); } }

        public void add<T>(string key, T value)
        {
            this.values.Add(key, new Pair<Type, object>(typeof(T), value));
        }

        public void update<T>(string key, T value)
        {
            if (this.values.ContainsKey(key))
            {
                this.values[key] = new Pair<Type, object>(typeof(T), value);
            }
            else
            {
                add<T>(key, value);
            }
        }

        public void remove(string key)
        {
            this.values.Remove(key);
        }

        public void clear()
        {
            this.values.Clear();
        }

        public Type getType(string key)
        {
            return values[key].first;
        }

        public object getValue(string key)
        {
            if (values.ContainsKey(key))
            {
                if (values[key].first == typeof(double))
                    return ((double)values[key].second).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                else
                    return values[key].second;
            }

            return null;
        }
    }
}
