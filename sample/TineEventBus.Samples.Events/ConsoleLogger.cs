using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TineEventBus.Samples.Events
{
    public class ConsoleLogger : IConsoleLogger
    {
        public void Write(string text) => Console.WriteLine(text);
    }
}
