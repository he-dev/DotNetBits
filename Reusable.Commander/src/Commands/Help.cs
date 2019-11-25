using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Commander.Utilities;
using Reusable.Data.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Utilities;
using t = Reusable.Commander.ConsoleTemplates;

namespace Reusable.Commander.Commands
{
    [PublicAPI]
    [Tags("h", "?")]
    [Description("Display help.")]
    public class Help : Command<Help.Parameter>
    {
        private static readonly int IndentWidth = 4;

        private static readonly int[] ColumnWidths = { 27, 50 };

        public Help(ILogger<Help> logger) : base(logger) { }

        public ConsoleStyle Style { get; set; }

        protected override Task ExecuteAsync(Parameter? parameter, CancellationToken cancellationToken)
        {
            var commandSelected = parameter.Command.IsNotNullOrEmpty();
            if (commandSelected)
            {
                RenderParameterList(parameter);
            }
            else
            {
                RenderCommandList(parameter);
            }

            return Task.CompletedTask;
        }

        protected virtual void RenderCommandList(Parameter parameter)
        {
            // Headers
            var captions = new[] { "Command", "Description" }.Pad(ColumnWidths);
            Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = captions });

            // Separators
            var separators = captions.Select(c => new string('-', c.Trim().Length)).Pad(ColumnWidths);
            Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = separators });

            // Commands
            var userExecutableCommands =
                from commandType in parameter.Commands
                where !commandType.IsDefined(typeof(InternalAttribute))
                orderby CommandHelper.GetMultiName(commandType).First()
                select commandType;

            foreach (var commandType in userExecutableCommands)
            {
                var defaultId = CommandHelper.GetMultiName(commandType).First();
                var aliases = string.Join("|", CommandHelper.GetMultiName(commandType).Skip(1).Select(x => x.ToString()));
                var description = commandType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "N/A";
                var row = new[] { $"{defaultId} ({(aliases.Length > 0 ? aliases : "-")})", description }.Pad(ColumnWidths);
                Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = row });
            }
        }

        protected virtual void RenderParameterList(Parameter parameter)
        {
            var commandType = parameter.Commands.SingleOrDefault(t => CommandHelper.GetMultiName(t).Equals(parameter.Command));
            if (commandType is null)
            {
                throw DynamicException.Create
                (
                    $"CommandNotFound",
                    $"Could not find a command with the name '{parameter.Command}'"
                );
            }

            // Headers
            var captions = new[] { "Option", "Description" }.Pad(ColumnWidths);
            Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = captions });

            // Separators
            var separators = captions.Select(c => new string('-', c.Trim().Length)).Pad(ColumnWidths);
            Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = separators });

            var bagType = commandType.GetCommandParameterType();
            var commandParameterProperties =
                from commandArgument in bagType.GetCommandParameterType().GetParameterProperties()
                orderby commandArgument.Name.First()
                select commandArgument;

            foreach (var commandParameterProperty in commandParameterProperties)
            {
                var defaultId = commandParameterProperty.Name.First();
                var aliases = string.Join("|", commandParameterProperty.Name.Skip(1).Select(x => x.ToString()));
                var description = commandParameterProperty.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "N/A";
                var row = new[] { $"{defaultId} ({(aliases.Length > 0 ? aliases : "-")})", description }.Pad(ColumnWidths);
                Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = row });
            }
        }

        [PublicAPI]
        public class Parameter : CommandParameter
        {
            [Description("Display command usage.")]
            [Alias("cmd")]
            [Position(1)]
            public string? Command { get; set; }

            [Service]
            public TypeList<ICommand> Commands { get; set; }
        }
    }

    internal static class HelpExtensions
    {
        public static IEnumerable<string> Pad(this IEnumerable<string> values, IEnumerable<int> columnWidths)
        {
            foreach (var (value, width) in values.Zip(columnWidths, (x, w) => (Value: x, Width: w)))
            {
                yield return Pad(value, width);
            }

            string Pad(string value, int width)
            {
                return
                    width < 0
                        ? value.PadLeft(-width, ' ')
                        : value.PadRight(width, ' ');
            }
        }

        public static string Required(this string value) => $"<{value}>";

        public static string Optional(this string value) => $"[{value}]";
    }
}

/*
 
NAME                ABOUT
----
command1     Does this.
cmd1             N/A

---

NAME

    command1 
    cmd1
    
ABOUT

    Does this.

SYNTAX

    command1 <1:arg1> <2:arg2> <arg3> <arg3> [arg4] [arg5]

ARGUMENTS

    arg1         blah
    arg2         blah     
     
     
     */