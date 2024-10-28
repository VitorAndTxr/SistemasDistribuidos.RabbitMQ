using Microsoft.Extensions.Configuration;
internal class Program
{
    private static void Main(string[] args)
    {
        ProntoSocorroServices prontoSocorroServices = new ProntoSocorroServices();
        prontoSocorroServices.ListenEmergencyMedicalResponseQueueAsync();

        while (true)
        {
            Console.WriteLine("Pressione qualquer tecla para solicitar um médico especialista");
            Console.ReadKey();
            prontoSocorroServices.RequestSpecialistDoctor("Requisição");
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
