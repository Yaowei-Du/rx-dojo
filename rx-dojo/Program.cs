using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

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

            Console.WriteLine("Finished process.");
        }

        private static void ProcessAsync(Queue<ExampleClass> queue)
        {
            var isCompleted = false;
            StreamObservable.MessageObservable(queue)
                .Select(ProcessMessage)
                .ObserveOn(NewThreadScheduler.Default)
                .Select(ProcessMessage2)
                .ObserveOn(NewThreadScheduler.Default)
                .Select(ProcessMessage3)
                .Subscribe(obj => { },
                    error =>
                    {
                        Console.WriteLine($"On error: {error.Message}");
                        isCompleted = true;
                    },
                    () =>
                    {
                        Console.WriteLine("On complete.");
                        isCompleted = true;
                    });

            SpinWait.SpinUntil(() => isCompleted);
            Console.WriteLine("Finished async.");
        }

        private static ExampleClass ProcessMessage(ExampleClass obj)
        {
            Process(obj, "Method1");
            return obj;
        }

        private static ExampleClass ProcessMessage2(ExampleClass obj)
        {
            Process(obj, "Method2");
            return obj;
        }

        private static ExampleClass ProcessMessage3(ExampleClass obj)
        {
            Process(obj, "Method3", 500);
            return obj;
        }

        private static void Process(ExampleClass obj, string methodName, int taskTime = 100)
        {
            Console.WriteLine(
                $"Message: {obj.ObjName}, in {methodName}#begin, Thread: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(new Random().Next(10) * taskTime);
            Console.WriteLine(
                $"Message: {obj.ObjName}, in {methodName}#end, Thread: {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}