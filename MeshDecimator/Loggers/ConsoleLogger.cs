using System;

namespace MeshDecimator.Loggers
{
    /// <summary>
    /// The default console logger.
    /// </summary>
    public sealed class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Logs a line of verbose text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void LogVerbose(string text)
        {
            Console.WriteLine(text);
        }

        /// <summary>
        /// Logs a line of warning text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void LogWarning(string text)
        {
            Console.WriteLine(text);
        }

        /// <summary>
        /// Logs a line of error text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void LogError(string text)
        {
            Console.Error.WriteLine(text);
        }
    }
}