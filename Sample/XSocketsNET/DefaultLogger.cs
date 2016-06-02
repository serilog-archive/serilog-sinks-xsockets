﻿using System.IO;
using Serilog;
using XSockets.Logger;

public class DefaultLogger : XLogger
{
    public DefaultLogger()
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose()
            .WriteTo.ColoredConsole()
            //.WriteTo.RollingFile(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Log\\Log-{Date}.txt"))
            .WriteTo.XSockets()
            .CreateLogger();
    }
}