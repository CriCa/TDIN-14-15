using System;

[Serializable]
public class User // basic class to store users
{
    public string Name { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    private int Balance;

    public User(string name, string username, string password)
    {
        Name = name;
        Username = username;
        Password = password;
        Balance = 100;
    }
}
