﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Diagnostics;
using Reusable.IOnymous;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Attachments;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Utilities.NLog.LayoutRenderers;

//[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(Person))]

namespace Reusable.Apps
{
    public static class Demo
    {
        public static void ConsoleColorizer()
        {
            SmartPropertiesLayoutRenderer.Register();

            var loggerFactory = new LoggerFactory
            {
                Observers =
                {
                    new ColoredConsoleRx(),
                },
                Configuration = new LoggerFactoryConfiguration
                {
                    Attachments = new HashSet<ILogAttachment>
                    {
                        new Timestamp<DateTimeUtc>(),
                    }
                }
            };

            var consoleLogger = loggerFactory.CreateLogger("ConsoleTemplateTest");

            consoleLogger.WriteLine(m => m
                .text(">")
                .span(s => s.text("foo").color(ConsoleColor.Red))
                .text(" bar ")
                .span(s => s
                    .text("foo ")
                    .span(ss => ss.text("bar").backgroundColor(ConsoleColor.Gray))
                    .text(" baz")
                    .backgroundColor(ConsoleColor.DarkYellow)
                )
            );
        }

        public static void SemanticExtensions()
        {
            SmartPropertiesLayoutRenderer.Register();

            //var fileProvider = new RelativeResourceProvider(new PhysicalFileProvider(), typeof(Demo).Assembly.Location);

            var loggerFactory =
                new LoggerFactory()
                    .AttachObject("Environment", "Demo")
                    .AttachObject("Product", "Reusable.Apps.Console")
                    .AttachScope()
                    .AttachSnapshot()
                    .Attach<Timestamp<DateTimeUtc>>()
                    .AttachElapsedMilliseconds()
                    .AddObserver<NLogRx>();
            //.UseConfiguration(LoggerFactoryConfiguration.Load(fileProvider.GetFileInfo(@"cfg\omnilog.json").CreateReadStream()));

            var logger = loggerFactory.CreateLogger("Demo");

            logger.Log(Abstraction.Layer.Infrastructure().Routine("SemLogTest").Running());
            logger.Log(Abstraction.Layer.Infrastructure().Meta(new { Null = (string)null }));

            
                
            // Opening outer-transaction.
            using (logger.BeginScope().WithCorrelationHandle("Blub").WithCorrelationContext(new { Name = "OuterScope", CustomerId = 123 }).AttachElapsed())
            {
                // Logging some single business variable and a message.
                logger.Log(Abstraction.Layer.Business().Variable(new { foo = "bar" }), log => log.Message("Hallo variable!"));
                logger.Log(Abstraction.Layer.Database().Counter(new { RowCount = 7 }));
                logger.Log(Abstraction.Layer.Database().Decision("blub").Because("bluby"));                

                // Opening inner-transaction.
                using (logger.BeginScope().WithCorrelationContext(new { Name = "InnerScope", ItemId = 456 }).AttachElapsed())
                {
                    var correlationIds = logger.Scopes().CorrelationIds<string>().ToList();
                    
                    // Logging an entire object in a single line.
                    var customer = new { FirstName = "John", LastName = "Doe" };
                    logger.Log(Abstraction.Layer.Business().Variable(new { customer }));

                    // Logging multiple variables in a single line.
                    var baz = 123;
                    var qux = "quux";

                    logger.Log(Abstraction.Layer.Infrastructure().Composite(new { multiple = new { baz, qux } }));

                    // Logging action results.
                    logger.Log(Abstraction.Layer.Infrastructure().Routine("DoSomething").Running());
                    logger.Log(Abstraction.Layer.Infrastructure().Routine("DoSomething").Canceled().Because("No connection."));
                    logger.Log(Abstraction.Layer.Infrastructure().Routine("DoSomething").Faulted(), new DivideByZeroException("Cannot divide."));
                }
            }
        }

        public static void Converters()
        {
            var converter =
                TypeConverter.Empty
                    .Add<StringToInt32Converter>()
                    .Add<StringToDateTimeConverter>();

            var result = converter.Convert("123", typeof(int));
        }

        public static void DebuggerDisplay()
        {
            var person = new Person
            {
                FirstName = "John",
                LastName = null,
                Age = 123.456,
                DBNullTest = DBNull.Value,
                GraduationYears = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                Nicknames = { "Johny", "Doe" }
            };

            //var toStringNull = default(Person).ToDebuggerDisplayString(builder => { builder.Property(x => x.FirstName); });

            var toString = person.ToDebuggerDisplayString(builder =>
            {
                builder.Property(x => x.FirstName, "{0,8}");
                builder.DisplayMember(x => x.LastName);
                builder.DisplayMember(x => x.DBNullTest);
                builder.DisplayMember(x => x.GraduationYears.Sum());
                builder.Property(x => x.Age, "{0:F2}");
                builder.DisplayMember(x => x.GraduationYears.Count);
                builder.DisplayCollection(x => x.GraduationYears);
                builder.DisplayCollection(x => x.GraduationYears, x => x, "{0:X2}");
                builder.DisplayCollection(x => x.Nicknames);
            });
        }
    }

    public class Person
    {
        private string _testField = "TestValue";

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public double Age { get; set; }

        public object DBNullTest { get; set; } = DBNull.Value;

        public IList<int> GraduationYears { get; set; } = new List<int>();

        public IList<string> Nicknames { get; set; } = new List<string>();

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayMember(x => x.FirstName);
            builder.DisplayMember(x => x.LastName);
            builder.DisplayMember(x => x._testField);
            builder.DisplayMember(x => x.DBNullTest);
            builder.DisplayMember(x => x.Age.ToString("F2"));
            builder.Property(x => x.Age, "{0:F2}");
            builder.DisplayMember(x => x.GraduationYears.Count);
            builder.DisplayCollection(x => x.GraduationYears);
            builder.DisplayCollection(x => x.GraduationYears, x => x, "{0:X2}");
            builder.DisplayCollection(x => x.Nicknames);
        });
    }
}