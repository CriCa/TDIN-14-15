using System;

/**
 * Class that represents a Diginote
 */
[Serializable]
public class Diginote 
{
    private static int nextSerial = 0; // static id of next diginote

    public static int NextSerial
    {
        get { return nextSerial; }
        set { nextSerial = value; }
    }

    private User user; // owner of the diginote

    public int Id { get; set; } // id of the diginote

    public User Owner
    {
        get
        {
            return user;
        }
        set
        {
            user = value;
            LastAquiredOn = DateTime.Now.ToString(); // when the owner changes, then set LastAquired
        }
    }

    public double Value { get; set; } // value of the diginote

    public string LastAquiredOn { get; set; } // date when the owner changed for the last time

    // constructor
    public Diginote(User user, double value = 1.0)
    {
        Owner = user;
        Id = nextSerial++;
        Value = value;
    }
}