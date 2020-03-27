using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
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
            var managementClient = new ManagementClient(connectionString);
            await managementClient.DeleteQueueAsync(queueName);
            await managementClient.CreateQueueAsync(new QueueDescription(queueName)
            {
                LockDuration = TimeSpan.FromMinutes(2), RequiresSession = true
            });

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
            var sessionClient = new SessionClient(connectionString, queueName, ReceiveMode.PeekLock);

            var expectedLockMessage = RandomBreakingMessage().ToList();

            while (true)
            {
                IMessageSession session;
                try
                {
                    session = await sessionClient.AcceptMessageSessionAsync("session", TimeSpan.FromMinutes(1));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                var queue = new Queue<QueueMessage>();
                try
                {
                    await ReceiveMessagesAsync(session, queue, 10);

                    foreach (var message in new Queue<QueueMessage>(queue))
                    {
                        await ProcessMessage(message, expectedLockMessage);
                        await ConsumeComplete(session, queue, message.LockToken);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await session.CloseAsync();
                }
            }
        }

        private static IEnumerable<string> RandomBreakingMessage()
        {
            var random = new Random();
            var messages = Enumerable.Range(1, 5).Select(i => random.Next(1, 25).ToString()).ToList();
            Console.WriteLine($"message should be break: {string.Join(",", messages)}");
            return messages;
        }

        private static async Task ProcessMessage(QueueMessage message, IList<string> expectedLockMessage)
        {
            var exampleClass = JsonConvert.DeserializeObject<ExampleClass>(Encoding.UTF8.GetString(message.Body));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"{DateTime.UtcNow:o} Message processing:" +
                $"\n\tSessionId = {message.SessionId}, " +
                $"\n\tMessageId = {message.MessageId}, " +
                $"\n\tSequenceNumber = {message.Message.SystemProperties.SequenceNumber}," +
                $"\n\tContent: [ item = {exampleClass.ObjName} ]"
            );
            if (expectedLockMessage.Contains(exampleClass.ObjName))
            {
                expectedLockMessage.Remove(exampleClass.ObjName);
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
                Console.ForegroundColor = ConsoleColor.Blue;
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