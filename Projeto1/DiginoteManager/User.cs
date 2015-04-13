using System;
// basic class to store users
using System.Collections;
public class User
{
    public string Name { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public User(string name, string username, string password)
    {
        Name = name;
        Username = username;
        Password = password;
    }
    /*public int CompareTo(object obj) {
        if (obj == null) return 1;

        Console.WriteLine("asdasd");
        User otherUser = obj as User;
        if (otherUser != null) 
            return this.Username.CompareTo(otherUser.Username) & this.Password.CompareTo(otherUser.Password);
        else 
           throw new ArgumentException("Object is not a User");
    }*/
}
