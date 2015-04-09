using System;
using System.Collections;

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