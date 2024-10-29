using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemasDistribuidos.Cryptography;
using SistemasDistribuidos.RabbitMQ.Domain.Enums;
using System.Text;
using System.Text.Json;

namespace SistemasDistribuidos.RabbitMQ.Enfermaria
{
    public class EnfermariaService
    {
        private readonly IConfigurationRoot _configuration;
        public EnfermariaService()
        {
            _configuration = InitConfigurations();   
        }

        public void PatientRelease(string message = "Paciente Liberado")
        {
            MessageEncodeProtocol messageEncodeProtocol = new MessageEncodeProtocol();

            Console.WriteLine("PatientRelease");

            using var connection = RabbitMQConnector.CreateConnection();

            using var channel = RabbitMQConnector.CreateChannel(connection);

            channel.ExchangeDeclare(_configuration[key: "PatientReleaseExchangeName"].ToLower(), type: ExchangeType.Direct);

            var privateKey = _configuration["PrivateKey"];

            var encodedMessage = messageEncodeProtocol.MountMessage(message, privateKey, AplicationNamesEnum.Enfermaria.ToString(), RoutineKeyNamesEnum.PatientRelease);

            var body = Encoding.UTF8.GetBytes(encodedMessage);

            channel.BasicPublish(_configuration[key: "PatientReleaseExchangeName"].ToLower(),
                                 routingKey: RoutineKeyNamesEnum.PatientRelease.ToString().ToLower(),
                                 basicProperties: null,
                                 body: body);

        }

        public void RequestSpecialistDoctor(string message = "Solicitar Médico")
        {
            MessageEncodeProtocol messageEncodeProtocol = new MessageEncodeProtocol();

            Console.WriteLine("RequestSpecialistDoctor");

            using var connection = RabbitMQConnector.CreateConnection();

            using var channel = RabbitMQConnector.CreateChannel(connection);

            channel.ExchangeDeclare(_configuration[key: "SpecialistDoctorRequestExchangeName"].ToLower(), type: ExchangeType.Direct);

            var privateKey = _configuration["PrivateKey"];

            var encodedMessage = messageEncodeProtocol.MountMessage(message, privateKey, AplicationNamesEnum.Enfermaria.ToString(), RoutineKeyNamesEnum.RequestSpecialistDoctor);

            var body = Encoding.UTF8.GetBytes(encodedMessage);

            channel.BasicPublish(_configuration[key: "SpecialistDoctorRequestExchangeName"].ToLower(),
                                 routingKey: RoutineKeyNamesEnum.RequestSpecialistDoctor.ToString().ToLower(),
                                 basicProperties: null,
                                 body: body);
        }

        public Task ListenRoomRequest()
        {
            return Task.Run(() =>
            {
                IConfigurationRoot config = InitConfigurations();

                var factory = new ConnectionFactory { HostName = "localhost" };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange: config["RoomRequestExchangeName"].ToLower(), type: ExchangeType.Direct);

                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                  exchange: config["RoomRequestExchangeName"].ToLower(),
                  routingKey: RoutineKeyNamesEnum.RoomRequest.ToString().ToLower());

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) => HandleRoomRequest(model, ea);

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
                // Loop de espera para manter a thread aberta
                while (true) { }
            });
        }

        public Task ListenSpecialistDoctorRequestAnwsers()
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
                  routingKey: RoutineKeyNamesEnum.NurseryMedicalResponse.ToString().ToLower());

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) => HandleSpecialistDoctorRequestAnwser(model, ea);

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
                // Loop de espera para manter a thread aberta
                while (true) { }
            });
        }
        static void HandleSpecialistDoctorRequestAnwser(object model, BasicDeliverEventArgs ea)
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
            var publicKey = config[("Nodes:" + messageHeader.SenderCode + "PublicKey")];

            if (!messageEncodeProtocol.ValidateMessage(messageHeader, publicKey))
                Console.WriteLine("Invalid signature");


            var routingKey = ea.RoutingKey;
        }

        static void HandleRoomRequest(object model, BasicDeliverEventArgs ea)
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
            var publicKey = config[("Nodes:" + messageHeader.SenderCode + "PublicKey")];

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
}