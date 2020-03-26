using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace rx_dojo
{
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new Queue<ExampleClass>();

            for (var i = 1; i < 10; i++)
            {
                queue.Enqueue(new ExampleClass(i.ToString()));
            }

            ProcessAsync(queue);

            Print("Finished process.");
        }

        private static void Print(string log)
        {
            Console.WriteLine($"{DateTime.UtcNow:o} {log}");
        }

        private static void ProcessAsync(Queue<ExampleClass> queue)
        {
            var isCompleted = false;
            var interval = 300;

            var counter = 1;

            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(interval), NewThreadScheduler.Default)
                .ObserveOn(NewThreadScheduler.Default)
                .Select(l => Observable.FromAsync(async _ =>
                {
                    Print($"starting deque. counter: {counter}");
                    counter += 1;
                    var exampleClass = await Dequeue(queue);
                    Print($"end deque. get message: {exampleClass.ObjName}");
                    return exampleClass;
                }))
                .Concat()
                .Select(ProcessMessage)
                .ObserveOn(NewThreadScheduler.Default)
                .Select(ProcessMessage2)
                .ObserveOn(NewThreadScheduler.Default)
                .Select(ProcessMessage3)
                .Subscribe(obj => { },
                    error =>
                    {
                        Print($"On error: {error.Message}");
                        isCompleted = true;
                    },
                    () =>
                    {
                        Print("On complete.");
                        isCompleted = true;
                    });

            SpinWait.SpinUntil(() => isCompleted);
            Print("Finished async.");
        }

        private static async Task<ExampleClass> Dequeue(Queue<ExampleClass> queue)
        {
            await Task.Delay(1000);
            return queue.Dequeue();
        }

        private static readonly Func<ExampleClass, ExampleClass> ProcessMessage = obj => Process("Method1", 100, obj);

        private static readonly Func<ExampleClass, Either<Error, ExampleClass>> ProcessMessage2 = obj =>
        {
            if (obj.ObjName == "5")
                return Error.New($"Object {obj.ObjName} validation failed.");
            return Process("Method2", 100, obj);
        };

        private static readonly Func<Either<Error, ExampleClass>, Either<Error, ExampleClass>> ProcessMessage3 =
            either => map(either, curry(Process)("Method3")(500));

        private static readonly Func<string, int, ExampleClass, ExampleClass> Process = (methodName, taskTime, obj) =>
        {
            Print(
                $"Message: {obj.ObjName}, in {methodName}#begin, Thread: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(new Random().Next(10) * taskTime);
            Print(
                $"Message: {obj.ObjName}, in {methodName}#end, Thread: {Thread.CurrentThread.ManagedThreadId}");
            return obj;
        };
    }
}