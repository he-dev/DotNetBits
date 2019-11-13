using System;
using System.Linq;
using System.Threading;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Rx;
using Reusable.OmniLog.Scalars;
using Xunit;

//using Reusable.OmniLog.Attachments;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class LoggerMiddlewareTest
    {
        [Fact]
        public void Can_add_nodes_after()
        {
            var nodes = new[] { new Node(1), new Node(2), new Node(3) };
            var last = nodes.Aggregate<ILoggerNode>((current, next) => current.AddAfter(next));
            
            Assert.Same(last, nodes.Last());
        }

        private class Node : ILoggerNode
        {
            public Node(int id) => Id = id;

            public int Id { get; }

            public bool Enabled { get; set; }

            public ILoggerNode Prev { get; set; }

            public ILoggerNode Next { get; set; }

            public void Invoke(LogEntry request) => throw new NotImplementedException();

            public void Dispose() => throw new NotImplementedException();
        }

        [Fact]
        public void Can_log_message()
        {
            var rx = new MemoryRx();
            using var lf = new LoggerFactory
            {
                Nodes =
                {
                    new StopwatchNode(),
                    new ScalarNode(),
                    new LambdaNode(),
                    new ScopeNode(),
                    new SerializerNode(),
                    //new LoggerFilter()
                    //new BufferNode(),
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                }
            };
            var logger = lf.CreateLogger("test");
            logger.Log(l => l.Message("Hallo!"));
            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]);
        }

        [Fact]
        public void Can_log_scope()
        {
            ExecutionContext.SuppressFlow();

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new StopwatchNode(),
                    new ScalarNode(),
                    new LambdaNode(),
                    new ScopeNode(),
                    new SerializerNode(),
                    //new LoggerFilter()
                    //new BufferNode(),
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                var outerCorrelationId = "test-id-1";
                using (logger.BeginScope(outerCorrelationId))
                {
                    var scope1 = logger.Scope();
                    logger.Log(l => l.Message("Hallo!"));
                    Assert.Same(outerCorrelationId, scope1.CorrelationId);
                    Assert.NotNull(rx[0][LogEntry.Names.Scope]);

                    var innerCorrelationId = "test-id-2";
                    using (logger.BeginScope(innerCorrelationId))
                    {
                        var scope2 = logger.Scope();
                        logger.Log(l => l.Message("Hi!"));
                        Assert.Same(innerCorrelationId, scope2.CorrelationId);
                        Assert.NotNull(rx[1][LogEntry.Names.Scope]);
                    }
                }

                Assert.Equal(2, rx.Count());
                Assert.Equal("Hallo!", rx[0]["Message"]);
                Assert.Equal("Hi!", rx[1]["Message"]);
            }
        }

        [Fact]
        public void Can_serialize_data()
        {
            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new StopwatchNode(),
                    new ScalarNode(),
                    new LambdaNode(),
                    new ScopeNode(),
                    new OneToManyNode(),
                    new SerializerNode(),
                    //new LoggerFilter()
                    //new BufferNode(),
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!").Snapshot(new { Greeting = "Hi!" }));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]);
            Assert.Equal("Greeting", rx.First()[LogEntry.Names.Object]);
            Assert.Equal("Hi!", rx.First()[LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);
            //Assert.Equal("{\"Greeting\":\"Hi!\"}", rx.First()["Snapshot"]);
        }

        [Fact]
        public void Can_attach_timestamp()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new ScalarNode
                    {
                        Functions =
                        {
                            new Timestamp(new[] { timestamp })
                        }
                    },
                    new LambdaNode(),
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!"));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]);
            Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        [Fact]
        public void Can_enumerate_dump()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new ScalarNode
                    {
                        Functions =
                        {
                            new Timestamp(new[] { timestamp })
                        }
                    },
                    new LambdaNode(),
                    new OneToManyNode(),
                    // new LoggerForward
                    // {
                    //     Routes =
                    //     {
                    //         ["Variable"] = "Identifier",
                    //         ["Dump"] = "Snapshot"
                    //     }
                    // },
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                },
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Snapshot(new Person { FirstName = "John", LastName = "Doe" }));
            }

            Assert.Equal(2, rx.Count());
            Assert.Equal("FirstName", rx[0][LogEntry.Names.Object]);
            Assert.Equal("John", rx[0][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);
            Assert.Equal("LastName", rx[1][LogEntry.Names.Object]);
            Assert.Equal("Doe", rx[1][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        [Fact]
        public void Can_map_snapshot()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new ScalarNode
                    {
                        Functions =
                        {
                            new Timestamp(new[] { timestamp })
                        }
                    },
                    new LambdaNode(),
                    new OneToManyNode(),
                    new MapperNode
                    {
                        Mappings =
                        {
                            MapperNode.Mapping.For<Person>(p => new { FullName = p.LastName + ", " + p.FirstName })
                        }
                    },
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                },
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Snapshot(new Person { FirstName = "John", LastName = "Doe" }, explodable: false));
            }

            Assert.Equal(1, rx.Count());
            //Assert.Equal("Hallo!", rx.First()["Message"]);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}