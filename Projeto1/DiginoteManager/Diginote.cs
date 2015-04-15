using System;


// diginote class
[Serializable]
public class Diginote 
{
    static int nextSerial = 0;
    private User user;

    public Diginote(User user)
    {
        this.user = user;
        Id = nextSerial++;
        Value = 1.0;
    }

  
    public int Id { get; set; }

    public User Owner
    {
        get
        {
            return user;
        }
        set
        {
            user = value;
            LastAquiredOn = DateTime.Now.ToString();
        }
    }

    public double Value { get; set; }

    public static int NextSerial
    {
        get { return nextSerial; }
        set { nextSerial = value; }
    }

    public string LastAquiredOn { get; set; }

}