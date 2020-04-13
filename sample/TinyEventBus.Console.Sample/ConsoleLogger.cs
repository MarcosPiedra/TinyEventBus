using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTinyEventBus
{
    public class ConsoleLogger : IConsoleLogger
    {
        public void Write(string text) => Console.WriteLine(text);
    }
}
