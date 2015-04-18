using System;

/**
 * wrapper class that repeats the calls to events
 */
public class ChangeEventRepeater : MarshalByRefObject
{
    public event ChangeDelegate ChangeEvent;

    public void Repeater(ChangeArgs args)
    {
        if (ChangeEvent != null)
            ChangeEvent(args);
    }

    // overiding lifetime to infinite
    public override object InitializeLifetimeService() { return null; }
}