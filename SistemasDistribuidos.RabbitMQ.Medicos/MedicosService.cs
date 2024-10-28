using System.Text;
using Microsoft.Extensions.Configuration;
using SistemasDistribuidos.Cryptography;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemasDistribuidos.RabbitMQ.Domain.Enums;

public class MedicosService
{
    private readonly IConfigurationRoot _configuration;
    public MedicosService()
    {
        _configuration = InitConfigurations();
    }

    public void AnswerSpecialistDoctorRequest(string message, RoutineKeyNamesEnum listener)
    {
        MessageEncodeProtocol messageEncodeProtocol = new MessageEncodeProtocol();
        Console.WriteLine("AnswerSpecialistDoctorRequest");

        using var connection = RabbitMQConnector.CreateConnection();

        using var channel = RabbitMQConnector.CreateChannel(connection);

        channel.ExchangeDeclare(_configuration[key: "MedicalRequestExchangeName"].ToLower(), type: ExchangeType.Fanout);

        var privateKey = _configuration["PrivateKey"];

        var encodedMessage = messageEncodeProtocol.MountMessage(message, privateKey, AplicationNamesEnum.ProntoSocorro.ToString(), RoutineKeyNamesEnum.RequestSpecialistDoctor);

        var body = Encoding.UTF8.GetBytes(encodedMessage);

        channel.BasicPublish(exchange: _configuration[key: "MedicalRequestExchangeName"].ToLower(),
                             routingKey: listener.ToString(),
                             basicProperties: null,
                             body: body);
        Console.WriteLine($"Sent MedicalResponse:{message}");
    }

    public void AnswerAdmission()
    {
        IConfigurationRoot config = InitConfigurations();

        using var connection = RabbitMQConnector.CreateConnection();

        using var channel = RabbitMQConnector.CreateChannel(connection);

        RabbitMQConnector.DeclareExchange(config[key: "MedicalRequestExchangeName"], channel);

    }

    public Task ListenEmergencyMedicalRequestQueue()
    {
        return Task.Run(() =>
        {
            IConfigurationRoot config = InitConfigurations();

            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: config[key: "MedicalRequestExchangeName"].ToLower(), type: ExchangeType.Direct);

            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName,
                      exchange: config[key: "MedicalRequestExchangeName"].ToLower(),
                      routingKey: RoutineKeyNamesEnum.RequestSpecialistDoctor.ToString().ToLower());


            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>

            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                Console.WriteLine($" [x] Received {message}");

            };
            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
            while (true) { }
        });
    }

    static IConfigurationRoot InitConfigurations()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // Define o diretório base onde está o appsettings.json
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true) // Carrega o arquivo appsettings.json
            .Build();
        return config;
    }
}