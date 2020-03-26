namespace rx_dojo
{
    class Program
    {
        public static void Main(string[] args)
        {
            const string connectionString = "";
            const string queueName = "";
            Sessions.Run(connectionString, queueName).GetAwaiter().GetResult();
        }
    }
}