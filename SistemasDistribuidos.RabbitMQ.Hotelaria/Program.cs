namespace SistemasDistribuidos.RabbitMQ.Hotelaria
{

    internal class Program
    {
        private static void Main(string[] args)
        {
            HotelariaService hotelariaService = new HotelariaService();
            hotelariaService.ListenRequestAdmission();
            hotelariaService.ListenPatientRelease();


            while (true)
            {
                Console.WriteLine("Pressione qualquer tecla para solicitar um médico especialista");
                Console.ReadKey();
                hotelariaService.RequestRoom();
            }
        }
    }
}