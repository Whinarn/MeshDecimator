using UnityEngine;

namespace MeshDecimator.Unity.Loggers
{
    /// <summary>
    /// A unity logger.
    /// </summary>
    public sealed class UnityLogger : ILogger
    {
        /// <summary>
        /// Logs a line of verbose text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void LogVerbose(string text)
        {
            Debug.Log(text);
        }

        /// <summary>
        /// Logs a line of warning text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void LogWarning(string text)
        {
            Debug.LogWarning(text);
        }

        /// <summary>
        /// Logs a line of error text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void LogError(string text)
        {
            Debug.LogError(text);
        }
    }
}