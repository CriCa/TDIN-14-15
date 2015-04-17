using System;
using System.Collections;
using System.Runtime.Remoting;

class Server
{
    static void Main(string[] args)
    {
        // set configuration
        RemotingConfiguration.Configure("Server.exe.config", false);
        
        Console.WriteLine("[Server]: Server started! Press return to stop!\n");
        // wait to stop
        Console.ReadLine();
    }
}