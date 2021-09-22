using System;

namespace ChatProgram
{
    public static class Logger
    {
        public static void Log(string message)
        {
            var dateTime = DateTime.UtcNow;
            Console.WriteLine($"[{dateTime:dd/MM/yy HH:mm:ss}] {message}");
        }
    }
}
