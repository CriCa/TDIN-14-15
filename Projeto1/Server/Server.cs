using System;
using System.Collections;
using System.Runtime.Remoting;

class Server
{
    static void Main(string[] args)
    {
        RemotingConfiguration.Configure("Server.exe.config", false);
        DiginoteTradingSystem sys = new DiginoteTradingSystem();
        sys.RegisterUser(new User("oiN", "oiU", "oiP"));
        sys.Login(new User("oiN", "oiU", "oiP"));
        Console.WriteLine("Logged Users: {0}", sys.getLoggedUsers().Count);
        sys.Logout(new User("oiN", "oiU", "oiP"));
        Console.WriteLine("Logged Users: {0}", sys.getLoggedUsers().Count);


        
        Console.WriteLine("[Server]: Server started! Press return to exit!\n");
        Console.ReadLine();

    }
}