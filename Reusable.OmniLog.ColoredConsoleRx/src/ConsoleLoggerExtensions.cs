﻿using System;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public static class ConsoleLoggerExtensions
    {
        public static ILogger Write(this ILogger logger, Func<HtmlElement, HtmlElement> paragraphAction)
        {
            var html = paragraphAction(HtmlElement.Builder.span()).ToHtml(HtmlFormatting.Empty);

            return logger.Log(log => log.With(LogProperties.Level, LogLevel.Information).With(ColoredConsoleRx.TemplatePropertyName, html));
        }
        
        public static ILogger WriteLine(this ILogger logger, Func<HtmlElement, HtmlElement> paragraphAction)
        {
            var html = paragraphAction(HtmlElement.Builder.p()).ToHtml(HtmlFormatting.Empty);

            return logger.Log(log => log.With(LogProperties.Level, LogLevel.Information).With(ColoredConsoleRx.TemplatePropertyName, html));
        }       
    }
}