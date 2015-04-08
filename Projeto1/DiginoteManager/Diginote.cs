using System;

// diginote class
public class Diginote
{
    static int nextSerial = 0;

    public Diginote()
    {
        Id = nextSerial++;
        Value = 1.0;
    }

    public Diginote(int id)
    {
        Id = id;
    }

    public int Id { get; set; }

    public double Value { get; set; }

    public static int NextSerial
    {
        get { return nextSerial; }
        set { nextSerial = value; }
    }
}