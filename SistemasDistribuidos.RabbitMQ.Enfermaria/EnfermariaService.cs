using Microsoft.Extensions.Configuration;

namespace SistemasDistribuidos.RabbitMQ.Enfermaria
{
    public class EnfermariaService
    {
        private readonly IConfigurationRoot _configuration;
        public EnfermariaService()
        {
            _configuration = InitConfigurations();   
        }

        public void PatientRelease()
        {

        }

        public void RequestSpecialistDoctor()
        {

        }

        public void ListenRoomRequest()
        {

        }

        public void ListenSpecialistDoctorRequestAnwsers()
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