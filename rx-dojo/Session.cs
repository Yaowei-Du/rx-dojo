using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace rx_dojo
{
    public static class Sessions
    {
        public static async Task Run(string connectionString, string sessionQueueName)
        {
            await SendMessagesAsync("session", connectionString, sessionQueueName);
            await InitializeReceiver(connectionString, sessionQueueName);
        }

        private static async Task SendMessagesAsync(string sessionId, string connectionString, string queueName)
        {
            var sender = new MessageSender(connectionString, queueName);

            var data = Enumerable.Range(1, 25).Select(i => new ExampleClass(i.ToString()));

            foreach (var item in data)
            {
                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item)))
                {
                    SessionId = sessionId,
                    ContentType = "application/json",
                    Label = "RecipeStep",
                    MessageId = item.ObjName,
                };
                await sender.SendAsync(message);
                lock (Console.Out)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Message sent: Session {0}, MessageId = {1}", message.SessionId,
                        message.MessageId);
                    Console.ResetColor();
                }
            }
        }

        private static async Task InitializeReceiver(string connectionString, string queueName)
        {
            var queue1 = new Queue<QueueMessage>();

            var sessionClient = new SessionClient(connectionString, queueName, ReceiveMode.PeekLock);
            var session = await sessionClient.AcceptMessageSessionAsync("session");

            await ReceiveMessagesAsync(session, queue1, 10);
            foreach (var message in new Queue<QueueMessage>(queue1))
            {
                try
                {
                    await ProcessMessage(message, "4");
                    await ConsumeComplete(session, queue1, message.LockToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }
            var queue2 = new Queue<QueueMessage>();
            await ReceiveMessagesAsync(session, queue2, 10);
            foreach (var message in new Queue<QueueMessage>(queue1))
            {
                try
                {
                    await ProcessMessage(message, "20");
                    await ConsumeComplete(session, queue1, message.LockToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            } 
        }

        private static async Task ProcessMessage(QueueMessage message, string expectedLockMessage)
        {
            var exampleClass = JsonConvert.DeserializeObject<ExampleClass>(Encoding.UTF8.GetString(message.Body));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"{DateTime.UtcNow:o} Message received:" +
                $"\n\tSessionId = {message.SessionId}, " +
                $"\n\tMessageId = {message.MessageId}, " +
                $"\n\tSequenceNumber = {message.Message.SystemProperties.SequenceNumber}," +
                $"\n\tContent: [ item = {exampleClass.ObjName} ]"
            );
            if (exampleClass.ObjName == expectedLockMessage)
            {
                await Task.Delay(130000);
            }

            await Task.Delay(3000);
        }

        public static async Task ReceiveMessagesAsync(IMessageSession session,
            Queue<QueueMessage> queue,
            int count)
        {
            var messages = await session.ReceiveAsync(count) ?? new List<Message>();
            var queueMessages = messages.Where(m => m != null)
                .Select(m => new QueueMessage(m));
            foreach (var message in queueMessages)
            {
                var exampleClass =
                    JsonConvert.DeserializeObject<ExampleClass>(Encoding.UTF8.GetString(message.Body));
                Console.WriteLine(
                    $"{DateTime.UtcNow:o} Message received:" +
                    $"\n\tSessionId = {message.SessionId}, " +
                    $"\n\tMessageId = {message.MessageId}, " +
                    $"\n\tSequenceNumber = {message.Message.SystemProperties.SequenceNumber}," +
                    $"\n\tContent: [ item = {exampleClass.ObjName} ]"
                );
                queue.Enqueue(message);
            }
        }

        public static async Task ConsumeComplete(IMessageSession session, Queue<QueueMessage> queue, string lockToken)
        {
            await session.CompleteAsync(lockToken);
            queue.Dequeue();
        }
    }
}