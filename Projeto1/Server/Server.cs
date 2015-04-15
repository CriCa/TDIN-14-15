using System;
using System.Collections;
using System.Runtime.Remoting;

class Server
{
    static void Main(string[] args)
    {
        RemotingConfiguration.Configure("Server.exe.config", false);
        
        DiginoteTradingSystem sys = new DiginoteTradingSystem();
        
        /*sys.RegisterUser("oiN", "oiU", "oiP");
        sys.Login("oiU", "oiP");
        Console.WriteLine("Logged Users: {0}", sys.getLoggedUsers().Count);
        sys.Logout(new User("oiN", "oiU", "oiP"));
        Console.WriteLine("Logged Users: {0}", sys.getLoggedUsers().Count);
        sys.saveState();
        sys.saveState();
        sys.RegisterUser("oiN", "oiU", "oiP");
        Console.WriteLine("[Server]: Server started! Press return to exit!\n\n\n######\n");
        sys = new DiginoteTradingSystem();
        sys.saveState();*/
        Console.WriteLine("[Server]: Server started! Press return to exit!\n");
        Console.ReadLine();
    }
}