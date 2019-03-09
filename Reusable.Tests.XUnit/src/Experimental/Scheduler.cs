using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Custom;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.Experimental
{
    public class SchedulerTest_console
    {
        [Fact]
        public async Task Go()
        {
            //var cronString = "5,10/5,15-30,45-50/2 * * 1-22 JAN,MAR-SEP MON-WED,4,FRI 2017";

            //CronExpression.Parse("0,10/5,5,45-50/2,15-30 * * 1-22   JAN,MAR-SEP * 2017").Dump();

            var timestamp = new DateTime(2017, 3, 1, 8, 0, 0);
            //	for (int i = 0; i < 120; i++)
            //	{
            //		//Console.WriteLine($"{timestamp.AddSeconds(i)} {cronExpr.Contains(timestamp.AddSeconds(i))}");	
            //	}

            // seconds minutes hours day-of-month month day-of-week year

            //CronExpression.Parse("* 0/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 0, 0)).Dump();
            //CronExpression.Parse("* 0/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 1, 0)).Dump();
            //CronExpression.Parse("* 0/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 5, 0)).Dump();

            //CronExpression.Parse("* 1/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 5, 0)).Dump();
            //CronExpression.Parse("* 1/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 6, 0)).Dump();
            //CronExpression.Parse("* 1/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 10, 0)).Dump();
            //CronExpression.Parse("* 1/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 11, 0)).Dump();
            //CronExpression.Parse("* 0/6 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 0, 0)).Dump();

            //var startedOn = DateTime.UtcNow;

            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            var scheduler = SchedulerFactory.CreateUtc();

            //Thread.Sleep(3300);

            //	scheduler.Schedule("0/1 * * * * * *", async schedule =>
            //	{
            //		Log(1, schedule);
            //		await Task.Delay(1100);
            //	}, maxDegreeOfParallelism: Scheduler.UnlimitedJobParallelism);
            //
            //	scheduler.Schedule("0/2 * * * * * *", async schedule =>
            //	{
            //		Log(2, schedule);
            //		await Task.Delay(1500);
            //
            //	}, maxDegreeOfParallelism: Scheduler.UnlimitedJobParallelism);

            var j3 = scheduler.Schedule(new Job("Single-job", new[] { new CronTrigger("0/3 * * * * * *") }, async token =>
            {
                //Log(3, DateTime.Now);
                await Task.Delay(4000);
                Console.WriteLine("Job-3: Finished!");
            })
            {
                MaxDegreeOfParallelism = 1
            });

            Console.WriteLine("Press ENTER to disconnect...");
            Console.ReadLine();
            j3.Dispose();
            scheduler.Dispose();
            //await scheduler.Continuation;
        }
    }

    public static class Tick
    {
        public static IObservable<DateTime> EverySecond(IDateTime dateTime)
        {
            return
                Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(_ => dateTime.Now());
        }
    }

    public static class ObservableExtensions
    {
        public static IObservable<DateTime> FixMissingSeconds(this IObservable<DateTime> tick, IDateTime dateTime)
        {
            var last = dateTime.Now().TruncateMilliseconds();

            return tick.SelectMany(_ =>
            {
                var now = dateTime.Now().TruncateMilliseconds();
                var gap = (now - last).Ticks / TimeSpan.TicksPerSecond;

                // If we missed one second due to time inaccuracy, 
                // this makes sure to publish the missing second too
                // so that all jobs at that second can also be triggered.
                return
                    Enumerable
                        .Range(0, (int)gap)
                        .Select(second => last = last.AddSeconds(1));
            });
        }
    }

//                #if DEBUG
//                 ticks
//                    .Do(timestamp => Console.WriteLine($" Tick: {timestamp:yyyy-MM-dd HH:mm:ss.fff}"))
//                    .Finally(() => Console.WriteLine("Disconnected!"))
//                #endif

    public class Scheduler : IDisposable
    {
        private readonly IConnectableObservable<DateTime> _scheduler;

        private readonly IDisposable _disconnect;

        public Scheduler(IObservable<DateTime> ticks)
        {
            // Not using .RefCount here because it should be ticking regardless of subscriptions.
            _scheduler = ticks.Publish();
            _disconnect = _scheduler.Connect();
        }

        public IDisposable Schedule(Job job, CancellationToken cancellationToken = default)
        {
            var unschedule =
                _scheduler
                    // .ToList the results so that all triggers have the chance to evaluate the tick.
                    .Where(tick => job.Triggers.Select(t => t.Matches(tick)).ToList().Any(x => x))
                    .Subscribe(timestamp => job.Execute(cancellationToken));

            return Disposable.Create(() =>
            {
                job.Continuation.Wait(job.UnscheduleTimeout);
                unschedule.Dispose();
            });
        }

        public void Dispose()
        {
            // Stop ticking.
            _disconnect.Dispose();
        }
    }

    public static class SchedulerFactory
    {
        public static Scheduler CreateUtc()
        {
            var dateTime = new DateTimeUtc();
            var generator = Tick.EverySecond(dateTime).FixMissingSeconds(dateTime);
            return new Scheduler(generator);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime TruncateMilliseconds(this DateTime dateTime)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
        }
    }

    public static class SchedulerExtensions
    {
        public static IDisposable Schedule
        (
            this Scheduler scheduler,
            Trigger trigger,
            Func<CancellationToken, Task> action,
            int maxDegreeOfParallelism = 1,
            CancellationToken cancellationToken = default
        )
        {
            return scheduler.Schedule(new Job("Job", new[] { trigger }, action)
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            }, cancellationToken);
        }
    }

    // Let it be a class because it'll get more properties later.
    public abstract class Trigger
    {
        public static IEnumerable<Trigger> Empty => Enumerable.Empty<Trigger>();

        public abstract bool Matches(DateTime tick);
    }

    public class CronTrigger : Trigger
    {
        private readonly CronExpression _cronExpression;

        public CronTrigger(string cronExpression)
        {
            _cronExpression = CronExpression.Parse(cronExpression);
        }

        public string Schedule => _cronExpression.ToString();

        public override bool Matches(DateTime tick)
        {
            return _cronExpression.Contains(tick);
        }
    }

    public class CountTrigger : Trigger
    {
        public CountTrigger(int count)
        {
            Counter = new InfiniteCounter(count);
        }

        public IInfiniteCounter Counter { get; }

        public override bool Matches(DateTime tick)
        {
            Counter.MoveNext();
            return Counter.Position == InfiniteCounterPosition.Last;
        }
    }

    public class DegreeOfParallelism : Primitive<int>
    {
        private const int UnlimitedValue = -1;

        public DegreeOfParallelism(int value) : base(value) { }

        public static readonly DegreeOfParallelism Unlimited = new DegreeOfParallelism(UnlimitedValue);

        protected override void Validate(int value)
        {
            if (value == UnlimitedValue)
            {
                return;
            }

            if (value < 1)
            {
                throw new ArgumentException("Value must be positive.");
            }
        }

        public static implicit operator DegreeOfParallelism(int value) => new DegreeOfParallelism(value);
    }

    public class Job
    {
        private readonly List<Task> _tasks = new List<Task>();

        public Job(string name, IEnumerable<Trigger> trigger, Func<CancellationToken, Task> action)
        {
            Name = name;
            Triggers = trigger.ToList();
            Action = action;
        }

        public string Name { get; }

        public IEnumerable<Trigger> Triggers { get; }

        public Func<CancellationToken, Task> Action { get; }

        public Action<Job> OnMisfire { get; set; }

        public DegreeOfParallelism MaxDegreeOfParallelism { get; set; } = 1;

        public TimeSpan UnscheduleTimeout { get; set; }

        public Task Continuation => Task.WhenAll(_tasks).ContinueWith(_ => _tasks.Clear());

        public int Count => _tasks.Count;

        public void Execute(CancellationToken cancellationToken)
        {
            if (CanExecute())
            {
                var jobTask = Action(cancellationToken);
                _tasks.Add(jobTask);
                jobTask.ContinueWith(_ => _tasks.Remove(jobTask), cancellationToken);
            }
            else
            {
                OnMisfire?.Invoke(this);
            }
        }

        private bool CanExecute()
        {
            return
                MaxDegreeOfParallelism.Equals(DegreeOfParallelism.Unlimited) ||
                Count < MaxDegreeOfParallelism.Value;
        }
    }


    // -----------------------

    public class DuckObject<T> : DynamicObject
    {
        private static readonly DuckObject<T> Duck = new DuckObject<T>();

        public static TValue Quack<TValue>(Func<dynamic, dynamic> quack)
        {
            return (TValue)quack(Duck);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            throw new InvalidOperationException($"Cannot use an indexer on '{typeof(T)}' because static types do not have them.");
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var member = typeof(T).GetMember(binder.Name).SingleOrDefault();
            switch (member?.MemberType)
            {
                case MemberTypes.Field:
                    result = typeof(T).InvokeMember(binder.Name, BindingFlags.GetField, null, null, null);
                    break;
                case MemberTypes.Property:
                    result = typeof(T).InvokeMember(binder.Name, BindingFlags.GetProperty, null, null, null);
                    break;
                default:
                    throw new StaticMemberNotFoundException<T>(binder.Name);
            }

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var member = typeof(T).GetMember(binder.Name).SingleOrDefault();
            switch (member?.MemberType)
            {
                case MemberTypes.Method:
                    result = typeof(T).InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, null, args);
                    break;
                default:
                    throw new StaticMemberNotFoundException<T>(binder.Name);
            }

            return true;
        }
    }

    public class DuckObject
    {
        private static readonly ConcurrentDictionary<Type, dynamic> Cache = new ConcurrentDictionary<Type, dynamic>();

        public static TValue Quack<TValue>(Type type, Func<dynamic, dynamic> quack)
        {
            var duck = Cache.GetOrAdd(type, t => Activator.CreateInstance(typeof(DuckObject<>).MakeGenericType(type)));
            return (TValue)quack(duck);
        }
    }

    public class StaticMemberNotFoundException<T> : Exception
    {
        public StaticMemberNotFoundException(string missingMemberName)
            : base($"Type '{typeof(T)}' does not contain static member '{missingMemberName}'") { }
    }

    public class PrimitiveTest
    {
        [Fact]
        public void Supports_equality()
        {
            var userName = new UserName("Bob");
            Assert.Equal("Bob", userName);
        }

        [Fact]
        public void Throws_when_invalid_value()
        {
            Assert.ThrowsAny<Exception>(() => new UserName(null));
        }

        private class UserName : Primitive<string>
        {
            public UserName(string value) : base(value) { }

            protected override void Validate(string value)
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value must not be null or empty.");
            }
        }
    }

    public class JobTest
    {
        [Fact]
        public async Task Job_executes_no_more_than_specified_number_of_times()
        {
            var misfireCount = 0;
            var job = new Job("test", Enumerable.Empty<Trigger>(), async token => await Task.Delay(TimeSpan.FromSeconds(3), token))
            {
                OnMisfire = j => misfireCount++,
                MaxDegreeOfParallelism = 2
            };
            job.Execute(CancellationToken.None);
            job.Execute(CancellationToken.None);
            job.Execute(CancellationToken.None);
            Assert.Equal(2, job.Count);
            Assert.Equal(1, misfireCount);

            // Wait until all jobs are completed.
            await job.Continuation;

            Assert.Equal(0, job.Count);
        }
    }

    public class SchedulerTest
    {
        [Fact]
        public void Executes_job_according_to_triggers()
        {
            var job1ExecuteCount = 0;
            var job2ExecuteCount = 0;
            var misfireCount = 0;
            var subject = new Subject<DateTime>();
            var scheduler = new Scheduler(subject);
            
            var unschedule1 = scheduler.Schedule(new Job("test-1", new[] { new CountTrigger(2) }, async token =>
            {
                Interlocked.Increment(ref job1ExecuteCount);
                await Task.Delay(TimeSpan.FromSeconds(3), token);
            })
            {
                MaxDegreeOfParallelism = 2,
                OnMisfire = _ => Interlocked.Increment(ref misfireCount),
                UnscheduleTimeout = TimeSpan.FromSeconds(4)
            });
            
            var unschedule2 = scheduler.Schedule(new Job("test-2", new[] { new CountTrigger(3) }, async token =>
            {
                Interlocked.Increment(ref job2ExecuteCount);
                await Task.Delay(TimeSpan.FromSeconds(3), token);
            })
            {
                MaxDegreeOfParallelism = 2,
                OnMisfire = _ => Interlocked.Increment(ref misfireCount),
                UnscheduleTimeout = TimeSpan.FromSeconds(4)
            });

            // Scheduler was just initialized and should not have executed anything yet.
            Assert.Equal(0, job1ExecuteCount);
            Assert.Equal(0, job2ExecuteCount);

            // Tick once.
            subject.OnNext(DateTime.Now);

            // Still nothing should be executed.
            Assert.Equal(0, job1ExecuteCount);
            Assert.Equal(0, job2ExecuteCount);

            // Now tick twice...
            subject.OnNext(DateTime.Now);
            subject.OnNext(DateTime.Now);

            // Unschedule the job. This blocking call waits until all tasks are completed.
            unschedule1.Dispose();
            unschedule2.Dispose();

            // Tick once again. Nothing should be executed anymore.
            subject.OnNext(DateTime.Now);
            
            // ...this should have matched the two triggers.
            Assert.Equal(1, job1ExecuteCount);
            Assert.Equal(1, job2ExecuteCount);

            Assert.Equal(0, misfireCount);
        }
    }
}