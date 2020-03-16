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

            for (var i = 1; i < 1000; i++)
            {
                queue.Enqueue(new ExampleClass(i.ToString()));
            }

            getMessages(queue, 10).ToObservable()
                .Select(ProcessMessage)
                .ObserveOn(NewThreadScheduler.Default)
                .Select(ProcessMessage2)
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(ProcessMessage3);
        }

        static IEnumerable<ExampleClass> getMessages(Queue<ExampleClass> queue, int count)
        {
            var objs = new List<ExampleClass>();
            for (var i = 0; i < count; i++)
            {
                objs.Add(queue.Dequeue());
            }

            return objs;
        }

        private static ExampleClass ProcessMessage(ExampleClass obj)
        {
            Console.WriteLine($"Message: {obj.ObjName}, in Method1#begin, Thread: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(new Random().Next(10) * 100);
            Console.WriteLine($"Message: {obj.ObjName}, in Method1#end, Thread: {Thread.CurrentThread.ManagedThreadId}");
            return obj;
        }


        private static ExampleClass ProcessMessage2(ExampleClass obj)
        {
            Console.WriteLine($"Message: {obj.ObjName}, in Method2#begin, Thread: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(new Random().Next(10) * 100);
            Console.WriteLine($"Message: {obj.ObjName}, in Method2#end, Thread: {Thread.CurrentThread.ManagedThreadId}");
            return obj;
        }

        private static void ProcessMessage3(ExampleClass obj)
        {  
            Console.WriteLine($"Message: {obj.ObjName}, in Method3#begin, Thread: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(new Random().Next(10) * 100);
            Console.WriteLine($"Message: {obj.ObjName}, in Method3#end, Thread: {Thread.CurrentThread.ManagedThreadId}");
        }
    }

    class ExampleClass
    {
        public string ObjName { get; set; }

        public ExampleClass(string objName)
        {
            this.ObjName = objName;
        }
    }
}