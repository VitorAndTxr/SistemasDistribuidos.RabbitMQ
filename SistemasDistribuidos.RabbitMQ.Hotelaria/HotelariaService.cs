using System.Text;
using Microsoft.Extensions.Configuration;
using SistemasDistribuidos.Cryptography;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemasDistribuidos.RabbitMQ.Domain.Enums;
using System.Text.Json;


namespace SistemasDistribuidos.RabbitMQ.Hotelaria
{
    public class HotelariaService
    {
        private readonly IConfigurationRoot _configuration;

        public HotelariaService()
        {
            _configuration = InitConfigurations();
        }

        public void RequestRoom(string message = "Requisitando Quarto")
        {
            MessageEncodeProtocol messageEncodeProtocol = new MessageEncodeProtocol();

            Console.WriteLine("RequestRoom");

            using var connection = RabbitMQConnector.CreateConnection();

            using var channel = RabbitMQConnector.CreateChannel(connection);

            channel.ExchangeDeclare(_configuration[key: "RoomRequestExchangeName"].ToLower(), type: ExchangeType.Direct);

            var privateKey = _configuration["PrivateKey"];

            var encodedMessage = messageEncodeProtocol.MountMessage(message, privateKey, AplicationNamesEnum.Hotelaria.ToString(), RoutineKeyNamesEnum.RoomRequest);

            var body = Encoding.UTF8.GetBytes(encodedMessage);

            channel.BasicPublish(_configuration[key: "RoomRequestExchangeName"].ToLower(),
                                 routingKey: RoutineKeyNamesEnum.RoomRequest.ToString().ToLower(),
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine($"Sent MedicalRequest:{encodedMessage}");
        }

        public Task ListenRequestAdmission()
        {
            return Task.Run(() =>
            {
                IConfigurationRoot config = InitConfigurations();

                var factory = new ConnectionFactory { HostName = "localhost" };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange: config["PatientAdmissionExchangeName"].ToLower(), type: ExchangeType.Direct);

                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                                  exchange: config["PatientAdmissionExchangeName"].ToLower(),
                                  routingKey: RoutineKeyNamesEnum.PatientAdmissionRequest.ToString().ToLower());

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) => HandleRequestAdmission(model, ea);

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                // Loop de espera para manter a thread aberta
                while (true) { }
            });
        }

        public Task ListenPatientRelease()
        {
            return Task.Run(() =>
            {
                IConfigurationRoot config = InitConfigurations();

                var factory = new ConnectionFactory { HostName = "localhost" };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange: config["PatientReleaseExchangeName"].ToLower(), type: ExchangeType.Direct);

                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                                  exchange: config["PatientReleaseExchangeName"].ToLower(),
                                  routingKey: RoutineKeyNamesEnum.PatientRelease.ToString().ToLower());

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) => HandlePatientRelease(model, ea);

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                // Loop de espera para manter a thread aberta
                while (true) { }
            });
        }

        static void HandleRequestAdmission(object model, BasicDeliverEventArgs ea)
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
            var publicKey = config[(messageHeader.SenderCode + "PublicKey")];

            if (!messageEncodeProtocol.ValidateMessage(messageHeader, publicKey))
                Console.WriteLine("Invalid signature");


            var routingKey = ea.RoutingKey;
        }

        static void HandlePatientRelease(object model, BasicDeliverEventArgs ea)
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
            var publicKey = config[(messageHeader.SenderCode + "PublicKey")];

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