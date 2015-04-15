using System;
using System.IO;

public class Logger
{
    public const string LOG_FILENAME = "log.txt";
    private StreamWriter file;

    public Logger()
    {
        // if file exists append info
        //if (File.Exists(LOG_FILENAME))
        
        // if file doesn't exist creates it
        if (!File.Exists(LOG_FILENAME))
        /*{
            file = new StreamWriter(LOG_FILENAME, true);
            file.Close();
        }
        else // else create file*/
        {
            file = new StreamWriter(LOG_FILENAME);
            Log("Created log file!");
            file.Close();
            file.Dispose();
        }
    }

    public void Log(string msg)
    {
        // write msg to file
        file = new StreamWriter(LOG_FILENAME, true);
        file.WriteLine(DateTime.Now + ": " + msg);
        file.Flush();
        file.Close();
        file.Dispose();
        // show on console
        Console.WriteLine("[Log]: " + msg);
    }
}
