using System;
using System.IO;

public class Logger
{
    public const string LOG_FILENAME = "log.txt";
    private StreamWriter file;

    public Logger(DiginoteTradingSystem ds)
    {
        // creating the file WARNING: for now this is creating a new file
        // later on we need to change to append mode
        file = new StreamWriter(LOG_FILENAME);
        Log("Created log file");

        // subscribe events
        ds.QuotationChange += QuotationChangeHandler;
    }

    public void Log(string msg)
    {
        // write msg to file
        file.WriteLine(DateTime.Now + "|" + msg);
        file.Flush();

        // show on console
        Console.WriteLine("[Logger]: " + msg);
    }

    public void QuotationChangeHandler(QuotationChangeType type, double value)
    {
        // log the new quotation value
        if (type == QuotationChangeType.Up)
            Log("[Server]: Quotation value went up to: " + value);
        else
            Log("[Server]: Quotation value went down to: " + value);
    }
}
