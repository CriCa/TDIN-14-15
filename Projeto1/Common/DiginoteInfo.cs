using System;

[Serializable]
public class DiginoteInfo
{
    public int Serial { get; set; }

    public double Value { get; set; }

    public string AquiredOn { get; set; }

    public DiginoteInfo(int s, double v, string d)
    {
        Serial = s;
        Value = v;
        AquiredOn = d;
    }
}
