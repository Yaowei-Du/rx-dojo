﻿using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
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

            Console.WriteLine("Finished process.");
        }

        private static void ProcessAsync(Queue<ExampleClass> queue)
        {
            var isCompleted = false;
            StreamObservable.MessageObservable(queue)
                .Select(obj => Process("Method1", 100, obj))
                .ObserveOn(NewThreadScheduler.Default)
                .Select(obj => obj.ObjName == "5"
                    ? (Either<Error, ExampleClass>) Error.New($"Object {obj.ObjName} validation failed.")
                    : Process("Method2", 100, obj))
                .ObserveOn(NewThreadScheduler.Default)
                .Select(either => map(either, curry(Process)("Method3")(500)))
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

        private static readonly Func<string, int, ExampleClass, ExampleClass> Process = (methodName, taskTime, obj) =>
        {
            Console.WriteLine(
                $"Message: {obj.ObjName}, in {methodName}#begin, Thread: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(new Random().Next(10) * taskTime);
            Console.WriteLine(
                $"Message: {obj.ObjName}, in {methodName}#end, Thread: {Thread.CurrentThread.ManagedThreadId}");
            return obj;
        };
    }
}