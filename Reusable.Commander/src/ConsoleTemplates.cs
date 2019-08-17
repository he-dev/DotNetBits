using System;
using System.Collections.Generic;
using System.Linq.Custom;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Rx.ConsoleRenderers;

// ReSharper disable once CheckNamespace
namespace Reusable.Commander.ConsoleTemplates
{
    using static ConsoleColor;

//    public class Prompt : ConsoleTemplateBuilder
//    {
//        public DateTime Timestamp => DateTime.Now;
//
//        public override HtmlElement Build(ILog log) =>
//            HtmlElement
//                .Builder
//                .span(span => span.text($"[{Timestamp:yyyy-MM-dd HH:mm:ss}]>"));
//    }

    public class Indent : HtmlConsoleTemplateBuilder
    {
        private readonly int _depth;

        public Indent(int depth)
        {
            _depth = depth;
        }

        public int Width { get; set; } = 1;

        public override HtmlElement Build(LogEntry logEntry) =>
            HtmlElement
                .Builder
                .span(x => x.text(new string(' ', Width * _depth)));
    }

    namespace Help
    {
        public class TableRow : HtmlConsoleTemplateBuilder
        {
            public IEnumerable<string> Cells { get; set; }

            public override HtmlElement Build(LogEntry logEntry) =>
                HtmlElement
                    .Builder
                    .span(x => x
                        .text(Cells.Join(string.Empty)));
        }
    }


    public class Error : HtmlConsoleTemplateBuilder
    {
        public string Text { get; set; }

        public override HtmlElement Build(LogEntry logEntry) =>
            HtmlElement
                .Builder
                .span(x => x
                    .text(Text)).color(Red);
    }
}