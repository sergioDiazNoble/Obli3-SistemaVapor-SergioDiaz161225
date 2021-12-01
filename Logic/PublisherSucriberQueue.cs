using Data;
using Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;

namespace Logic
{
    public static class PublisherSucriberQueue
    {
        private static IModel channel;
        private const string QueueName = "LogsQueue";

        public static void StartChannel(IModel channelq)
        {
            channel = channelq;
            channel.QueueDeclare(queue: QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

        }

        public static void SendMessages(object obj)
        {
            PublishMessage(obj);
        }

        private static void PublishMessage(object obj)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));

            channel.BasicPublish(
            exchange: "",
            routingKey: QueueName,
            basicProperties: null,
            body: body);

        }

        public static void ReceiveMessages()
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Received message : " + message);
                var obj = JsonSerializer.Deserialize<Logs<object>>(message);
                if (obj != null)
                {
                    if (message.Contains("Domain.Game"))
                    {

                        await GameLogsRepo.AddAsync(obj.As<Game>());
                    }

                    if (message.Contains("Domain.User"))
                    {

                        await UserLogsRepo.AddAsync(obj.As<User>());
                    }
                }
            };

            channel.BasicConsume(
            queue: QueueName,
            autoAck: true,
            consumer: consumer);
        }
    }
}
