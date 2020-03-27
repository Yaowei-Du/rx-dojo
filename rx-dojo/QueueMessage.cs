using Microsoft.Azure.ServiceBus;

namespace rx_dojo
{
    public class QueueMessage
    {
        public QueueMessage()
        {
        }

        public QueueMessage(Message message)
        {
            Message = message;
            SessionId = message.SessionId;
            MessageId = message.MessageId;
            Body = message.Body;
            LockToken = message.SystemProperties.LockToken;
            
        }

        public string LockToken { get; set; }

        public Message Message;
        public string SessionId { get; set; }
        public string MessageId { get; set; }
        public byte[] Body { get; set; }
    }
}