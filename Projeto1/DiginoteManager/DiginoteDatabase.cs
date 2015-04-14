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

    public void AddDiginote(Diginote dig, string username)
    {
        diginotesOwners.Add(dig, username);
    }

    public void ChangeDiginoteOwner(Diginote dig, string username)
    {
        if (diginotesOwners.Contains(dig))
        {
            diginotesOwners.Remove(dig);
            diginotesOwners.Add(dig, username);
        }
            
       
    }

}