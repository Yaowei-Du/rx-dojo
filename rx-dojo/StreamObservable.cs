using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace rx_dojo
{
    public static class StreamObservable
    {
        public static IObservable<ExampleClass> MessageObservable(Queue<ExampleClass> queue)
        {
            return MessageStream(queue).ToObservable();
        }

        static IEnumerable<ExampleClass> MessageStream(Queue<ExampleClass> queue)
        {
            while (queue.Count > 0)
            {
                yield return queue.Dequeue();
            }
        }

        private static void StartGetMessage(Queue<ExampleClass> queue, IObserver<ExampleClass> o)
        {
            var exampleClasses = getMessages(queue, 5);
            foreach (var exampleClass in exampleClasses)
            {
                o.OnNext(exampleClass);
            }
        }

        private static IObservable<ExampleClass> EndlessQueueObservable(Queue<ExampleClass> queue)
        {
            return Observable.Create<ExampleClass>(o =>
            {
                try
                {
                    StartGetMessage(queue, o);
                    var timer = new System.Timers.Timer(4000);
                    timer.Elapsed += (_1, _2) => StartGetMessage(queue, o);
                    timer.Start();
                }
                catch (Exception e)
                {
                    o.OnError(e);
                }

                o.OnCompleted();
                return Task.CompletedTask;
            });
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
    }
}