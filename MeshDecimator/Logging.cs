using System;

namespace MeshDecimator
{
    #region ILogger
    /// <summary>
    /// A logger.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a line of verbose text.
        /// </summary>
        /// <param name="text">The text.</param>
        void LogVerbose(string text);

        /// <summary>
        /// Logs a line of warning text.
        /// </summary>
        /// <param name="text">The text.</param>
        void LogWarning(string text);

        /// <summary>
        /// Logs a line of error text.
        /// </summary>
        /// <param name="text">The text.</param>
        void LogError(string text);
    }
    #endregion

    /// <summary>
    /// The logging API.
    /// </summary>
    public static class Logging
    {
        #region Fields
        private static ILogger logger = null;
        private static object syncObj = new object();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the active logger.
        /// </summary>
        public static ILogger Logger
        {
            get { return logger; }
            set {
                lock (syncObj)
                {
                    logger = value;
                }
            }
        }
        #endregion

        #region Static Initializer
        /// <summary>
        /// The static initializer.
        /// </summary>
        static Logging()
        {
            logger = new Loggers.ConsoleLogger();
        }
        #endregion

        #region Public Methods
        #region Verbose
        /// <summary>
        /// Logs a line of verbose text.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void LogVerbose(string text)
        {
            lock (syncObj)
            {
                if (logger != null)
                {
                    logger.LogVerbose(text);
                }
            }
        }

        /// <summary>
        /// Logs a line of formatted verbose text.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="args">The string arguments.</param>
        public static void LogVerbose(string format, params object[] args)
        {
            LogVerbose(string.Format(format, args));
        }
        #endregion

        #region Warnings
        /// <summary>
        /// Logs a line of warning text.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void LogWarning(string text)
        {
            lock (syncObj)
            {
                if (logger != null)
                {
                    logger.LogWarning(text);
                }
            }
        }

        /// <summary>
        /// Logs a line of formatted warning text.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="args">The string arguments.</param>
        public static void LogWarning(string format, params object[] args)
        {
            LogWarning(string.Format(format, args));
        }
        #endregion

        #region Errors
        /// <summary>
        /// Logs a line of error text.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void LogError(string text)
        {
            lock (syncObj)
            {
                if (logger != null)
                {
                    logger.LogError(text);
                }
            }
        }

        /// <summary>
        /// Logs a line of formatted error text.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="args">The string arguments.</param>
        public static void LogError(string format, params object[] args)
        {
            LogError(string.Format(format, args));
        }
        #endregion
        #endregion
    }
}