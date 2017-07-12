﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.Logging.Loggex
{
    public static class LogEntryExtensions
    {
        #region Setters

        public static LogEntry LogLevel(this LogEntry entry, LogLevel logLevel) { entry[nameof(LogLevel)] = logLevel; return entry; }

        public static LogEntry Trace(this LogEntry entry) => entry.LogLevel(Loggex.LogLevel.Trace);
        public static LogEntry Debug(this LogEntry entry) => entry.LogLevel(Loggex.LogLevel.Debug);
        public static LogEntry Info(this LogEntry entry) => entry.LogLevel(Loggex.LogLevel.Info);
        public static LogEntry Warn(this LogEntry entry) => entry.LogLevel(Loggex.LogLevel.Warn);
        public static LogEntry Error(this LogEntry entry) => entry.LogLevel(Loggex.LogLevel.Error);
        public static LogEntry Fatal(this LogEntry entry) => entry.LogLevel(Loggex.LogLevel.Fatal);

        public static LogEntry Message(this LogEntry entry, string message) => entry.Message(b => b.Clear().Append(message));

        public static LogEntry Message(this LogEntry entry, Action<StringBuilder> builder)
        {
            var messageBuilder = entry.GetValueOrCreate(nameof(Message), () => new StringBuilder());
            builder(messageBuilder);
            return entry;
        }

        public static LogEntry Exception(this LogEntry entry, Exception exception) => entry.SetValue(nameof(Exception), exception);

        public static LogEntry Stopwatch(this LogEntry entry, Action<Stopwatch> stopwatch)
        {
            stopwatch(entry.GetValueOrCreate(nameof(Stopwatch), () => new Stopwatch()));
            return entry;
        }

        public static LogEntry Caller(this LogEntry entry, [CallerMemberName] string callerName = null) => entry.SetValue(nameof(Caller), callerName);

        public static LogEntry LineNumber(this LogEntry entry, [CallerLineNumber] int lineNumber = 0) => entry.SetValue(nameof(LineNumber), lineNumber);

        #endregion

        #region Getters

        public static LogLevel LogLevel(this LogEntry logEntry) => (LogLevel)logEntry[nameof(LogLevel)];

        public static CaseInsensitiveString Name(this LogEntry logEntry) => (CaseInsensitiveString)logEntry[nameof(ILogger.Name)];

        public static string Message(this LogEntry logEntry) => logEntry.GetValueOrCreate(nameof(Message), () => new StringBuilder()).ToString();

        #endregion

        [NotNull]
        private static T GetValueOrCreate<T>(this LogEntry entry, string name, Func<T> create)
        {
            return (T)(entry.TryGetValue(name, out var value) ? value : (entry[name] = create()));
        }

        [CanBeNull]
        public static T GetValueOrDefault<T>(this LogEntry entry, string name) => entry.TryGetValue(name, out object value) ? (T)value : default(T);

        [NotNull]
        public static LogEntry SetValue<T>(this LogEntry entry, CaseInsensitiveString name, T value)
        {
            (entry ?? throw new ArgumentNullException(nameof(entry)))[name] = value;
            return entry;
        }

        [NotNull]
        public static LogEntry SetProperty<T>(this LogEntry entry, T property) where T : IComputedProperty
        {
            return entry.SetValue(property.Name, property);
        }
    }
}
