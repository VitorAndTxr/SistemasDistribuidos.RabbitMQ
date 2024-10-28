using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemasDistribuidos.RabbitMQ.Domain.Enums;

internal class Program
{
    private static void Main(string[] args)
    {
        MedicosService medicosService = new MedicosService();
        medicosService.ListenEmergencyMedicalRequestQueue();

        while (true)
        {
            Console.WriteLine("Pressione qualquer tecla para responder uma solicitação especialista");
            Console.ReadKey();
            //medicosService.AnswerSpecialistDoctorRequest(RoutinKeyNamesEnum.NurseryMedicalResponse);
            medicosService.AnswerSpecialistDoctorRequest("Validado Emergencia",RoutineKeyNamesEnum.EmergencyMedicalResponse);
            medicosService.AnswerSpecialistDoctorRequest("Validado Enfermaria", RoutineKeyNamesEnum.NurseryMedicalResponse);
        }
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
