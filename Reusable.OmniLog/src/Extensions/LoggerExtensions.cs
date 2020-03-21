﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Collections.Generic;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Data;
using Reusable.OmniLog.Helpers;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Utilities;

// ReSharper disable ExplicitCallerInfoArgument - yes, we want to explicitly set it via overloads.

namespace Reusable.OmniLog
{
    [PublicAPI]
    public static class LoggerExtensions
    {
        #region LogLevels

        public static void Trace(this ILogger logger, string message, ProcessLogEntryDelegate? alter = null)
        {
            logger.Log(LogLevel.Trace, message, null, alter);
        }

        public static void Debug(this ILogger logger, string message, ProcessLogEntryDelegate? alter = null)
        {
            logger.Log(LogLevel.Debug, message, null, alter);
        }

        public static void Warning(this ILogger logger, string message, ProcessLogEntryDelegate? alter = null)
        {
            logger.Log(LogLevel.Warning, message, null, alter);
        }

        public static void Information(this ILogger logger, string message, ProcessLogEntryDelegate? alter = null)
        {
            logger.Log(LogLevel.Information, message, null, alter);
        }

        public static void Error(this ILogger logger, string message, Exception? exception = null, ProcessLogEntryDelegate? alter = null)
        {
            logger.Log(LogLevel.Error, message, exception, alter);
        }

        public static void Fatal(this ILogger logger, string message, Exception? exception = null, ProcessLogEntryDelegate? alter = null)
        {
            logger.Log(LogLevel.Fatal, message, exception, alter);
        }

        public static void Log(this ILogger logger, Action<ILogEntry> process)
        {
            logger.UseDelegate(process);
            logger.Log(new LogEntry());
        }

        public static void Log(this ILogger logger, params object[] items)
        {
            logger.UsePropertyFactory(items?.Where(x => x is {}));
            logger.Log(new LogEntry());
        }

        private static void Log
        (
            this ILogger logger,
            LogLevel level,
            string? message,
            Exception? exception,
            ProcessLogEntryDelegate? alter
        )
        {
            logger.Log(log =>
            {
                log.Level(level);
                if (message is {}) log.Message(message);
                if (exception is {}) log.Exception(exception);
                alter?.Invoke(log);
            });
        }

        #endregion

        #region Other

        public static T Return<T>(this ILogger logger, T obj)
        {
            return obj;
        }

        #endregion

        #region HtmlConsole

        /// <summary>
        /// Writes to console without line breaks.
        /// </summary>
        /// <param name="console">The logger to log.</param>
        /// <param name="style">Overrides the default console style.</param>
        /// <param name="builders">Console template builders used to render the output.</param>
        public static void Write(this ILogger console, IConsoleStyle style, params IHtmlConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(false, style, builders));
        }

        /// <summary>
        /// Writes to console line breaks.
        /// </summary>
        /// <param name="console">The logger to log.</param>
        /// <param name="style">Overrides the default console style.</param>
        /// <param name="builders">Console template builders used to render the output.</param>
        public static void WriteLine(this ILogger console, IConsoleStyle style, params IHtmlConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(true, style, builders));
        }

        private static ILogEntry ConsoleTemplateBuilder(this ILogEntry logEntry, bool isParagraph, IConsoleStyle style, IEnumerable<IHtmlConsoleTemplateBuilder> builders)
        {
            return logEntry.Add(Names.Default.Message, new HtmlConsoleTemplateBuilder(isParagraph, style, builders), m => m.ProcessWith<EchoNode>().LogWith<HtmlConsoleRx>());
        }

        #endregion

        public static T Node<T>(this ILoggerNode node) where T : ILoggerNode
        {
            return node.EnumerateNext().OfType<T>().SingleOrThrow(onEmpty: () => DynamicException.Create($"{nameof(LoggerNode)}NotFound", $"There was no {typeof(T).ToPrettyString()}."));
        }

        /// <summary>
        /// Gets logger-node of the specified type.
        /// </summary>
        public static T Node<T>(this ILogger logger) where T : ILoggerNode
        {
            return ((ILoggerNode)logger).Node<T>();
        }

        #region LoggerExtensions for LogEntryBuilder

        // We use context as the name and not abstractionContext because it otherwise interferes with intellisense.
        // The name abstractionContext appears first on the list and you need to scroll to get the Abstraction.
        public static void Log<T>
        (
            this ILogger logger,
            ILogEntryBuilder<T> context,
            ProcessLogEntryDelegate? alter = null,
            // These properties are for free so let's just log them too.
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.Log(log =>
            {
                log.Add(context.Name, context, m => m.ProcessWith<BuilderNode>());
                log.Add(Names.Default.CallerMemberName, callerMemberName!, m => m.ProcessWith<EchoNode>());
                log.Add(Names.Default.CallerLineNumber, callerLineNumber!, m => m.ProcessWith<EchoNode>());
                log.Add(Names.Default.CallerFilePath, Path.GetFileName(callerFilePath!), m => m.ProcessWith<EchoNode>());
                alter?.Invoke(log);
            });
        }

        public static void Log<T>
        (
            this ILogger logger,
            ILogEntryBuilder<T> context,
            string message,
            Exception exception,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.Message(message).Exception(exception),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log<T>
        (
            this ILogger logger,
            ILogEntryBuilder<T> context,
            string message,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.Message(message),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log<T>
        (
            this ILogger logger,
            ILogEntryBuilder<T> context,
            Exception exception,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.Exception(exception),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log
        (
            this ILogger logger,
            Action<ILogEntry> node,
            // These properties are for free so let's just log them too.
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.Log(log =>
            {
                node(log);
                log.Add(Names.Default.CallerMemberName, callerMemberName!, m => m.ProcessWith<EchoNode>());
                log.Add(Names.Default.CallerLineNumber, callerLineNumber!, m => m.ProcessWith<EchoNode>());
                log.Add(Names.Default.CallerFilePath, Path.GetFileName(callerFilePath!), m => m.ProcessWith<EchoNode>());
            });
        }

        #endregion
    }
}