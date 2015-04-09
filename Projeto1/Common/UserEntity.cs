using System;

[Serializable]
public class UserEntity // user entity identifier
{
    public UserEntity(string name, string nick)
    {
        Name = name;
        Nick = nick;
    }

    public string Name { get; set; }

    public string Nick { get; set; }
}