using Microsoft.Extensions.Configuration;

namespace SistemasDistribuidos.RabbitMQ.Hotelaria
{
    public class HotelariaService
    {
        private readonly IConfigurationRoot _configuration;

        public HotelariaService()
        {
            _configuration = InitConfigurations();
        }

        public void ListenRequestAdmission()
        {

        }

        public void ListenPatientRelease()
        {

        }

        public void RequestRoom()
        {

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