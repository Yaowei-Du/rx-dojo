using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;

namespace rx_dojo
{
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new Queue<ExampleClass>();

            for (var i = 0; i < 1000; i++)
            {
                queue.Enqueue(new ExampleClass(i.ToString()));
            }

            var messages = getMessages(queue, 20);
            messages.ToObservable()
                .Where(l =>
                {
                    if ((int.Parse(l.ObjName)) % 3 == 0)
                    {
                        Console.WriteLine($"start wait for 2s: {l.ObjName}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(2000));
                        Console.WriteLine($"end wait for 2s: {l.ObjName}");
                    }
                    return true;
                })
                .Where(l =>
                {
                    if ((int.Parse(l.ObjName)) % 5 == 0)
                    {
                        Console.WriteLine($"start wait for 5s: {l.ObjName}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(5000));
                        Console.WriteLine($"end wait for 5s: {l.ObjName}");
                        
                    }
                    return true;
                })
                .Subscribe(ProcessMessage);
//            messages.ToObservable()
//                .Buffer(5)
//                .Subscribe(ProcessMessages);
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
            Console.WriteLine(obj.ObjName);
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