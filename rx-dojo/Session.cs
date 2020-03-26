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

            var data = Enumerable.Range(1, 50).Select(i => new ExampleClass(i.ToString()));

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
            var queue = new Queue<Message>();

            var sessionClient = new SessionClient(connectionString, queueName, ReceiveMode.PeekLock);
            var sessionAsync = await sessionClient.AcceptMessageSessionAsync("session");

            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine("start receiving.");
                var receiveAsync = await sessionAsync.ReceiveAsync(10);
                Console.WriteLine("end receiving.");
                foreach (var message in receiveAsync)
                {
                    var exampleClass =
                        JsonConvert.DeserializeObject<ExampleClass>(Encoding.UTF8.GetString(message.Body));
                    Console.WriteLine(
                        $"{DateTime.UtcNow:o} Message received:" +
                        $"\n\tSessionId = {message.SessionId}, " +
                        $"\n\tMessageId = {message.MessageId}, " +
                        $"\n\tSequenceNumber = {message.SystemProperties.SequenceNumber}," +
                        $"\n\tContent: [ item = {exampleClass.ObjName} ]"
                    );
                    queue.Enqueue(message);
                }
            }

            foreach (var message in queue)
            {
                var exampleClass = JsonConvert.DeserializeObject<ExampleClass>(Encoding.UTF8.GetString(message.Body));
                Console.WriteLine(
                    $"{DateTime.UtcNow:o} Message received:" +
                    $"\n\tSessionId = {message.SessionId}, " +
                    $"\n\tMessageId = {message.MessageId}, " +
                    $"\n\tSequenceNumber = {message.SystemProperties.SequenceNumber}," +
                    $"\n\tContent: [ item = {exampleClass.ObjName} ]"
                );
                await Task.Delay(90000);
                await sessionAsync.CompleteAsync(message.SystemProperties.LockToken);
            }
        }
    }
}