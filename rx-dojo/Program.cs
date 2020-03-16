using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace rx_dojo
{
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new Queue<ExampleClass>();

            for (var i = 0; i < 40; i++)
            {
                queue.Enqueue(new ExampleClass(i.ToString()));
            }
            

//            var messages = getMessages(queue, 200);
            
            GenerateObservable(queue)
                .Where(l =>
                {
                    if ((int.Parse(l.ObjName)) % 3 == 0)
                    {
                        Console.WriteLine($"start wait for 0.2s: {l.ObjName} in {Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(200));
                    }
                    return true;
                })
                .ObserveOn(NewThreadScheduler.Default)
                .Where(l =>
                {
                    if ((int.Parse(l.ObjName)) % 5 == 0)
                    {
                        Console.WriteLine($"start wait for 0.5s: {l.ObjName} in {Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    }
                    return true;
                })
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(ProcessMessage);
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

        private static void ProcessMessage(ExampleClass obj)
        {
            Console.WriteLine($"~~~~~~{obj.ObjName} in {Thread.CurrentThread.ManagedThreadId}");
        }
        
        private static void ProcessMessages(IEnumerable<ExampleClass> objs)
        {
            foreach (var exampleClass in objs)
            {
                if (int.Parse(exampleClass.ObjName) % 3 == 0)
                {
                    Thread.Sleep(3000);
                }
                Console.WriteLine(exampleClass.ObjName);
            }
        }

        public static IObservable<ExampleClass> GenerateObservable(Queue<ExampleClass> queue)
        {
            return Observable.Create<ExampleClass>(observer =>
            {
                while (queue.Count != 0)
                {
                    var exampleClasses = getMessages(queue, 20);
                    var message = exampleClasses;
                    foreach (var exampleClass in message) observer.OnNext(exampleClass);
                }
                return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
            });
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