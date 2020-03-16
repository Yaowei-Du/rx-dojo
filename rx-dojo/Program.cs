using System;
using System.Collections.Generic;
using System.Reactive.Linq;

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
            Console.WriteLine(obj.ObjName);
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