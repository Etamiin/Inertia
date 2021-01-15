using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Realtime;

public class ExampleRealtime
{
    public static ExampleRealtime Instance { get; private set; }

    public ExampleRealtime()
    {
        if (Instance != null)
            return;

        Instance = this;

        //create a new ScriptCollection for adding scripts
        var collection = new ScriptCollection();

        //add a script
        /*var script = */ collection.Add<ExampleScript>(/*arguments passed to the Awake method in the script*/);

        //create an "ScriptInTime" instance
        //this class loop (permanently or not) the specified method in the specified time

        //Example: will log a message in the console after 3seconds one time
        new ScriptInTime(3f, () => Console.WriteLine("Hello world!"));

        //Example 2: will log a message in the console after 10seconds (permanently)
        new ScriptInTime(10f, () => Console.WriteLine("Announce: hello world!"), permanent: true);

    }
}