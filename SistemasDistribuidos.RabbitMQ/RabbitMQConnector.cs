using RabbitMQ.Client;

public class RabbitMQConnector
{
    public static IConnection CreateConnection()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = factory.CreateConnection();
        return connection;
    }

    public static IModel CreateChannel(IConnection connection)
    {
        var channel = connection.CreateModel();
        return channel;
    }
    public static void DeclareQueue(IModel channel, string queueName)
    {
        channel.QueueDeclare(queue: queueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
    }
    public static void DeclareExchange(string exchangeName, IModel channel)
    {
        channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout);
    }
}
