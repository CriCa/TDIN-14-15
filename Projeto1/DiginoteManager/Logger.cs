using System;
using System.IO;

public class Logger
{
    public const string LOG_FILENAME = "log.txt";
    private StreamWriter file;

    public Logger()
    {
        // if file exists append info
        if (File.Exists(LOG_FILENAME))
        {
            file = new StreamWriter(LOG_FILENAME, true);
        }
        else // else create file
        {
            file = new StreamWriter(LOG_FILENAME);
            Log("Created log file!");
        }
    }

    public void Log(string msg)
    {
        // write msg to file
        file.WriteLine(DateTime.Now + ": " + msg);
        file.Flush();

        // show on console
        Console.WriteLine("[Log]: " + msg);
    }
}
