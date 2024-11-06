using System.Text;
using Microsoft.Extensions.Configuration;
using SistemasDistribuidos.Cryptography;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemasDistribuidos.RabbitMQ.Domain.Enums;
using System.Text.Json;

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

        channel.ExchangeDeclare(_configuration[key: "SpecialistDoctorAnswerExchangeName"].ToLower(), type: ExchangeType.Direct);

        var privateKey = _configuration["PrivateKey"];

        var encodedMessage = messageEncodeProtocol.MountMessage(message, privateKey, AplicationNamesEnum.Medicos.ToString(), listener);

        var body = Encoding.UTF8.GetBytes(encodedMessage);

        channel.BasicPublish(exchange: _configuration[key: "SpecialistDoctorAnswerExchangeName"].ToLower(),
                             routingKey: listener.ToString().ToLower(),
                             basicProperties: null,
                             body: body);
    }

    public void RequestPatientAdmission()
    {
        MessageEncodeProtocol messageEncodeProtocol = new MessageEncodeProtocol();
        Console.WriteLine("RequestPatientAdmission");
        IConfigurationRoot config = InitConfigurations();

        using var connection = RabbitMQConnector.CreateConnection();

        using var channel = RabbitMQConnector.CreateChannel(connection);

        channel.ExchangeDeclare(_configuration[key: "PatientAdmissionExchangeName"].ToLower(), type: ExchangeType.Direct);

        var privateKey = _configuration["PrivateKey"];

        var message = "Request Patient Admission";
        var encodedMessage = messageEncodeProtocol.MountMessage(message, privateKey, AplicationNamesEnum.Medicos.ToString(), RoutineKeyNamesEnum.PatientAdmissionRequest);

        var body = Encoding.UTF8.GetBytes(encodedMessage);

        channel.BasicPublish(exchange: _configuration[key: "PatientAdmissionExchangeName"].ToLower(),
                             routingKey: RoutineKeyNamesEnum.PatientAdmissionRequest.ToString().ToLower(),
                             basicProperties: null,
                             body: body);
    }

    public Task ListenSpecialistDoctorRequestQueue()
    {
        return Task.Run(() =>
        {
            IConfigurationRoot config = InitConfigurations();

            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: config[key: "SpecialistDoctorRequestExchangeName"].ToLower(), type: ExchangeType.Direct);

            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName,
                      exchange: config[key: "SpecialistDoctorRequestExchangeName"].ToLower(),
                      routingKey: RoutineKeyNamesEnum.RequestSpecialistDoctor.ToString().ToLower());


            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) => HandleListenEmergencyMedicalRequest(model, ea);

            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
            while (true) { }
        });
    }

    static void HandleListenEmergencyMedicalRequest(object model, BasicDeliverEventArgs ea)
    {
        IConfigurationRoot config = InitConfigurations();

        MessageEncodeProtocol messageEncodeProtocol = new MessageEncodeProtocol();
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        MessageHeader messageHeader = JsonSerializer.Deserialize<MessageHeader>(message);

        Console.WriteLine($" [x] Received {message}");

        if (messageHeader == null)
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
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // Define o diretório base onde está o appsettings.json
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true) // Carrega o arquivo appsettings.json
            .Build();
        return config;
    }
}