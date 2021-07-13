using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

public class ExampleLogger
{
    public static ExampleLogger Instance { get; private set; }

    public ExampleLogger()
    {
        if (Instance != null)
            return;

        Instance = this;
        
        //Define CustomLogger as default logger
        BaseLogger.SetDefaultLogger<CustomLogger>();

        //Use logger

        //var customLogger = this.GetLogger() as CustomLogger;
        this.GetLogger().Log("Sample log");
        this.GetLogger().LogPattern("Example", "Log ExamplePattern");
        this.GetLogger().LogPattern("OK", "Success!");
    }
}