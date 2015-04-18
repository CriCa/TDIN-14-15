using System;

/**
 * Class that represents an user
 */
[Serializable]
public class User
{
    public string Name { get; set; } // name of the user

    public string Username { get; set; } // username of the user (unique)

    public string Password { get; set; } // password (always encrypted)

    // constructor
    public User(string name, string username, string password)
    {
        Name = name;
        Username = username;
        Password = password;
    }
}
