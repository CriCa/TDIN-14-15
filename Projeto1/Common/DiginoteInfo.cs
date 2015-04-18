using System;

/**
 * Class that describes a diginote
 */
[Serializable]
public class DiginoteInfo
{
    public int Serial { get; set; } // id of the diginote

    public double Value { get; set; } // value of the diginote

    public string AquiredOn { get; set; } // when was the diginote aquired by the current user

    // constructor
    public DiginoteInfo(int s, double v, string d)
    {
        Serial = s;
        Value = v;
        AquiredOn = d;
    }
}
