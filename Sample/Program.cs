using System;
using XSockets.Core.Common.Socket;
using XSockets.Plugin.Framework;

namespace Sample
{
    /// <summary>
    /// Since XSockets use Serilog the serilog XSockets sink will work directly.
    /// See - https://github.com/serilog/serilog-sinks-xsockets
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = Composable.GetExport<IXSocketServerContainer>())
            {
                container.Start();
                Console.ReadLine();
                container.Stop();
            }
        }
    }
}
