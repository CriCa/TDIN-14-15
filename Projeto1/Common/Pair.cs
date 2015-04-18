using System;

/**
 * Helper class that represents a pair structure
 */
[Serializable]
public class Pair<T, U>
{
    private T First;
    private U Second;

    public T first { get { return this.First; } set { this.First = value; } }
    public U second { get { return this.Second; } set { this.Second = value; } }

    public Pair(T first, U second)
    {
        this.first = first;
        this.second = second;
    }
}