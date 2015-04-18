using System;
using System.IO;

/**
 * Class that is responsible for logging changes from server to a file
 */
public class Logger
{
    public const string LOG_FILENAME = "log.txt"; // log file name
    private StreamWriter file; // stream instance

    public Logger()
    {
        // if file doesn't exist creates it
        if (!File.Exists(LOG_FILENAME))
        {
            file = new StreamWriter(LOG_FILENAME);
            Log("Created log file");
            Console.WriteLine("[Log]: Created log file");
        }
        else // else open stream with append flag
            file = new StreamWriter(LOG_FILENAME, true);
    }

    public void Log(string msg)
    {
        // write msg to file
        file.WriteLine(DateTime.Now + ": " + msg);
        file.Flush();

        // show on console
        // Console.WriteLine("[Log]: " + msg);
    }
}
