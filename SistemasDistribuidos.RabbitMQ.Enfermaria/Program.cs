namespace SistemasDistribuidos.RabbitMQ.Enfermaria
{

    internal class Program
    {
        private static void Main(string[] args)
        {
            EnfermariaService enfermariaService = new EnfermariaService();
            enfermariaService.ListenSpecialistDoctorRequestAnwsers();
            enfermariaService.ListenRoomRequest();

            while (true)
            {
                Console.WriteLine("Pressione qualquer tecla para solicitar um médico especialista");
                Console.ReadKey();
                enfermariaService.RequestSpecialistDoctor();
                enfermariaService.PatientRelease();

            }
        }
    }
}