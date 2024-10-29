using Microsoft.Extensions.Configuration;
using SistemasDistribuidos.RabbitMQ.Domain.Enums;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SistemasDistribuidos.Cryptography
{
    public class MessageEncodeProtocol
    {
        public static void Main()
        {
            string message = "Mensagem importante";

            // Gerar par de chaves RSA
            using (RSA rsa = RSA.Create())
            {
                // Gerar a chave privada
                byte[] privateKey = rsa.ExportRSAPrivateKey();
                byte[] publicKey = rsa.ExportRSAPublicKey();

                // Criar hash da mensagem
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    byte[] hash = sha256.ComputeHash(messageBytes);

                    // Assinar o hash com a chave privada
                    byte[] signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    Console.WriteLine("Assinatura: " + Convert.ToBase64String(signature));
                }
            }
        }

        public bool ValidateMessage(MessageHeader messageHeader, string publicKey)
        {
            if(messageHeader == null)
            {
                Console.WriteLine("");
                Console.WriteLine("Assinatura válida: " + false);
                Console.WriteLine("");
                return false;
            }

            byte[] publicKeyBytes = Convert.FromBase64String(publicKey);

            byte[] signatureBytes = Convert.FromBase64String(messageHeader.Signature);

            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(publicKeyBytes, out _);

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(messageHeader.Message);
                    byte[] hash = sha256.ComputeHash(messageBytes);

                    // Verificar a assinatura
                    bool isValid = rsa.VerifyHash(hash, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    Console.WriteLine("");
                    Console.WriteLine("Assinatura válida: " + isValid);
                    Console.WriteLine("");

                    return isValid;
                }

            }
        }

        public string MountMessage(string message, string signKeyBase64, string senderCode, RoutineKeyNamesEnum routineKey) 
        {
            MessageHeader messageHeader = new MessageHeader();

            byte[] signKey = Convert.FromBase64String(signKeyBase64);

            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(signKey, out _);

                // Criar hash da mensagem
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    byte[] hash = sha256.ComputeHash(messageBytes);

                    // Assinar o hash com a chave privada
                    byte[] signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    messageHeader.Message = message;
                    messageHeader.Signature = Convert.ToBase64String(signature);
                    messageHeader.SenderCode = senderCode;
                    messageHeader.routineKeyNames = routineKey;
                    Console.WriteLine("Message Enviada:" + JsonSerializer.Serialize(messageHeader));

                    Console.WriteLine("");
                    Console.WriteLine("");
                    

                    return JsonSerializer.Serialize(messageHeader);
                }
            }
        }
    }
    public class MessageHeader
    {
        public string Message { get; set; }
        public string SenderCode { get; set; }
        public string Signature { get; set; }
        public RoutineKeyNamesEnum routineKeyNames { get; set; }
    }


}
