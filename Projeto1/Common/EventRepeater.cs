using System;

public class ChangeEventRepeater : MarshalByRefObject
{
    public event ChangeDelegate ChangeEvent;

    public override object InitializeLifetimeService() { return null; }

    public void Repeater(ChangeArgs args)
    {
        if (ChangeEvent != null)
            ChangeEvent(args);
    }
}