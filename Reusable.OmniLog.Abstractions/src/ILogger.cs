using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogger
    {
        /// <summary>
        /// Gets middleware root.
        /// </summary>
        LoggerNode Node { get; }

        //T Use<T>(T next) where T : LoggerNode;

        void Log(LogEntry logEntry);
    }

    // ReSharper disable once UnusedTypeParameter - This is required for dependency-injection.
    public interface ILogger<T> : ILogger { }
}