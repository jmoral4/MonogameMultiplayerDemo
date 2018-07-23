using System;
using Sentry;

namespace MultiplayerDemo
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SentrySdk.Init("https://5fd7a6cda8444965bade9ccfd3df9882@sentry.io/1188141");

            using (var game = new Game1())
                game.Run();
        }
    }
}
