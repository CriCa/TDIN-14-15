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

    public static bool operator ==(Diginote b, Diginote c)
    {
        if (b.Id == c.Id)
            return true;
        return false;
    }

    public static bool operator !=(Diginote b, Diginote c)
    {
        if (b.Id != c.Id)
            return true;
        return false;
    }

    //public static bool Equals(Diginote b)
    public override bool Equals(System.Object obj)
    {
        if (((Diginote)obj).Id == this.Id)
            return true;
        return false;
    }
}