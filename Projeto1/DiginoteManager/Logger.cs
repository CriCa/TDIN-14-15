using System;
using System.IO;

public class Logger
{
    public const string LOG_FILENAME = "log.txt";
    private StreamWriter file;

    public Logger(ChangeDelegate cDelegate)
    {
        // creating the file WARNING: for now this is creating a new file
        // later on we need to change to append mode
        file = new StreamWriter(LOG_FILENAME);
        Log("Created log file");

        // subscribe events
        cDelegate += QuotationChangeHandler; // this has to be unsubcribed somewhere
    }

    public void Log(string msg)
    {
        // write msg to file
        file.WriteLine(DateTime.Now + "|" + msg);
        file.Flush();

        // show on console
        Console.WriteLine("[Logger]: " + msg);
    }

    public void QuotationChangeHandler(ChangeArgs args)
    {
        // log the new quotation value
        if (args.Type == ChangeType.QuotationUp)
            Log("[Server]: Quotation value went up to: " + args.QuotationValue);
        else if (args.Type == ChangeType.QuotationDown)
            Log("[Server]: Quotation value went down to: " + args.QuotationValue);
    }
}
