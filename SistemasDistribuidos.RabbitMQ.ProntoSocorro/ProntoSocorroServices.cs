using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemasDistribuidos.Cryptography;
using SistemasDistribuidos.RabbitMQ.Domain.Enums;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ProntoSocorroServices
{
    private readonly IConfigurationRoot _configuration;

    public ProntoSocorroServices()
    {
        _configuration = InitConfigurations();
    }
    public void RequestSpecialistDoctor(string message)
    {
        MessageEncodeProtocol messageEncodeProtocol = new MessageEncodeProtocol();

        Console.WriteLine("RequestSpecialistDoctor");

        using var connection = RabbitMQConnector.CreateConnection();

        using var channel = RabbitMQConnector.CreateChannel(connection);

        channel.ExchangeDeclare(_configuration[key: "SpecialistDoctorRequestExchangeName"].ToLower(), type: ExchangeType.Direct);

        var privateKey = _configuration["PrivateKey"];

        var encodedMessage = messageEncodeProtocol.MountMessage(message, privateKey, AplicationNamesEnum.ProntoSocorro.ToString(), RoutineKeyNamesEnum.RequestSpecialistDoctor);

        var body = Encoding.UTF8.GetBytes(encodedMessage);

        channel.BasicPublish(_configuration[key: "SpecialistDoctorRequestExchangeName"].ToLower(),
                             routingKey: RoutineKeyNamesEnum.RequestSpecialistDoctor.ToString().ToLower(),
                             basicProperties: null,
                             body: body);
    }

    public Task ListenSpecialistDoctorAnswerQueueAsync()
    {
        return Task.Run(() =>
        {
            IConfigurationRoot config = InitConfigurations();

            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: config["SpecialistDoctorAnswerExchangeName"].ToLower(), type: ExchangeType.Fanout);

            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName,
                              exchange: config["SpecialistDoctorAnswerExchangeName"].ToLower(),
                              routingKey: RoutineKeyNamesEnum.EmergencyMedicalResponse.ToString().ToLower());

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) => HandleEmergencyMedicalResponse(model, ea);

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            // Loop de espera para manter a thread aberta
            while (true) { }
        });
    }

    static void HandleEmergencyMedicalResponse(object model, BasicDeliverEventArgs ea)
    {
        IConfigurationRoot config = InitConfigurations();

        MessageEncodeProtocol messageEncodeProtocol = new MessageEncodeProtocol();
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        MessageHeader messageHeader = JsonSerializer.Deserialize<MessageHeader>(message);

        Console.WriteLine($" [x] Received {message}");

        if(messageHeader == null)
        {
            Console.WriteLine("Invalid message");
            return;
        }
        var publicKey = config[("Nodes:"+messageHeader.SenderCode + "PublicKey")];

        if (!messageEncodeProtocol.ValidateMessage(messageHeader, publicKey))
            Console.WriteLine("Invalid signature");


        var routingKey = ea.RoutingKey;
    }

    static IConfigurationRoot InitConfigurations()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // Define o diretório base onde está o appsettings.json
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true) // Carrega o arquivo appsettings.json
            .Build();
        return config;
    }
}