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
            medicosService.AnswerSpecialistDoctorRequest("Validado Emergencia",RoutineKeyNamesEnum.EmergencyMedicalResponse);
            medicosService.AnswerSpecialistDoctorRequest("Validado Enfermaria", RoutineKeyNamesEnum.NurseryMedicalResponse);
        }
    }
}
