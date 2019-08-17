using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.OmniLog
{
    public static class LoggerNodeTest
    {
        public class BuilderNodeTest { }

        public class CorrelationNodeTest
        {
            [Fact]
            public void Can_push_an_pop_scope()
            {
                var node = new CorrelationNode();
                var logEntry = new LogEntry();
                
                var key = new ItemKey<SoftString>(LogEntry.Names.Scope, LogEntry.Tags.Serializable);

                Assert.False(logEntry.TryGetItem<List<CorrelationNode.Scope>>(key, out var scope));


                using (node.Push((CorrelationId: "scope-1", CorrelationHandle: "handle-1")))
                {
                    node.Invoke(logEntry = new LogEntry());

                    Assert.True(logEntry.TryGetItem(key, out scope));
                    Assert.Equal(1, scope.Count);
                    Assert.Equal(new[] { "scope-1" }, scope.Select(x => x.CorrelationId));

                    using (node.Push((CorrelationId: "scope-2", CorrelationHandle: "handle-2")))
                    {
                        node.Invoke(logEntry = new LogEntry());

                        Assert.True(logEntry.TryGetItem(key, out scope));
                        Assert.Equal(2, scope.Count);
                        Assert.Equal(new[] { "scope-2", "scope-1" }, scope.Select(x => x.CorrelationId));

                        using (node.Push((CorrelationId: "scope-3", CorrelationHandle: "handle-3")))
                        {
                            node.Invoke(logEntry = new LogEntry());

                            Assert.True(logEntry.TryGetItem(key, out scope));
                            Assert.Equal(3, scope.Count);
                            Assert.Equal(new[] { "scope-3", "scope-2", "scope-1" }, scope.Select(x => x.CorrelationId));
                        }

                        node.Invoke(logEntry = new LogEntry());

                        Assert.True(logEntry.TryGetItem(key, out scope));
                        Assert.Equal(2, scope.Count);
                        Assert.Equal(new[] { "scope-2", "scope-1" }, scope.Select(x => x.CorrelationId));
                    }

                    node.Invoke(logEntry = new LogEntry());

                    Assert.True(logEntry.TryGetItem(key, out scope));
                    Assert.Equal(1, scope.Count);
                    Assert.Equal(new[] { "scope-1" }, scope.Select(x => x.CorrelationId));
                }

                node.Invoke(logEntry = new LogEntry());

                Assert.False(logEntry.TryGetItem(key, out scope));
            }
        }

        public class OneToManyNodeTest
        {
            [Fact]
            public void Can_enumerate_dictionary()
            {
                var node = new OneToManyNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(2);
                node.InsertNext(next);

                var requestKey = new ItemKey<SoftString>("test", LogEntry.Tags.Explodable);
                node.Invoke(new LogEntry().SetItem(requestKey, new Dictionary<string, object>
                {
                    ["a"] = "aa",
                    ["b"] = "bb"
                }));

                Assert.Equal(2, logs.Count);
                Assert.Equal("a", logs[0][LogEntry.Names.Object]);
                Assert.Equal("aa", logs[0][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);
                Assert.Equal("b", logs[1][LogEntry.Names.Object]);
                Assert.Equal("bb", logs[1][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);

                next.Assert();
            }

            [Fact]
            public void Can_enumerate_object_properties()
            {
                var node = new OneToManyNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(2);
                node.InsertNext(next);

                var requestKey = new ItemKey<SoftString>("test", LogEntry.Tags.Explodable);
                node.Invoke(new LogEntry().SetItem(requestKey, new
                {
                    a = "aaa",
                    b = "bbb"
                }));

                Assert.Equal(2, logs.Count);
                Assert.Equal("a", logs[0][LogEntry.Names.Object]);
                Assert.Equal("aaa", logs[0][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);
                Assert.Equal("b", logs[1][LogEntry.Names.Object]);
                Assert.Equal("bbb", logs[1][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);

                next.Assert();
            }

            [Fact]
            public void Does_nothing_to_string()
            {
                var node = new OneToManyNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(1);
                node.InsertNext(next);

                var requestKey = new ItemKey<SoftString>("test", LogEntry.Tags.Loggable);
                node.Invoke(new LogEntry().SetItem(requestKey, "abc"));

                Assert.Equal(1, logs.Count);
                //Assert.Equal("test", logs[0][LogEntry.Names.Object]);
                Assert.Equal("abc", logs[0][requestKey]);

                next.Assert();
            }
        }

        public class SerializerNodeTest
        {
            [Fact]
            public void Can_serialize_object()
            {
                var node = new SerializerNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(1);
                node.InsertNext(next);

                node.Invoke(new LogEntry().SetItem(SerializerNode.CreateRequestItemKey("test"), new { a = "2a" }));

                Assert.Equal(1, logs.Count);
                Assert.Equal(@"{""a"":""2a""}", logs[0]["test"]);
                
                next.Assert();
            }
        }

        public class BufferNodeTest
        {
            [Fact]
            public void Can_push_an_pop_scope()
            {
                var node = new BufferNode();

                var next = Mock.Create<LoggerNode>();
                next.Arrange(x => x.Invoke(Arg.IsAny<LogEntry>())).Occurs(3);
                node.InsertNext(next);

                using (var tran1 = node.Push())
                {
                    node.Invoke(new LogEntry());
                    Assert.Equal(1, tran1.Buffer.Count);

                    using (var tran2 = node.Push())
                    {
                        node.Invoke(new LogEntry());
                        node.Invoke(new LogEntry());

                        Assert.Equal(2, tran2.Buffer.Count);

                        using (var tran3 = node.Push())
                        {
                            node.Invoke(new LogEntry());
                            node.Invoke(new LogEntry());
                            node.Invoke(new LogEntry());

                            Assert.Equal(1, tran1.Buffer.Count);
                            Assert.Equal(2, tran2.Buffer.Count);
                            Assert.Equal(3, tran3.Buffer.Count);

                            tran3.Flush();

                            Assert.Equal(1, tran1.Buffer.Count);
                            Assert.Equal(2, tran2.Buffer.Count);
                            Assert.Equal(0, tran3.Buffer.Count);
                        }

                        Assert.Equal(1, tran1.Buffer.Count);
                        Assert.Equal(2, tran2.Buffer.Count);
                    }

                    Assert.Equal(1, tran1.Buffer.Count);
                }

                next.Assert();
            }
        }
    }
}