using System;
using System.Collections;

// not really necessary, can be done in diginotesystem class
public class DiginoteDatabase
{
    private Hashtable diginotesOwners;

    public DiginoteDatabase()
    {
        diginotesOwners = new Hashtable();
    }

    public void AddDiginote(Diginote dig, string user)
    {
        diginotesOwners.Add(dig, user);
    }
}