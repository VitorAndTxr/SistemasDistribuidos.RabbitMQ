// See https://aka.ms/new-console-template for more information
using System;
using System.Security.Cryptography;


        // Gerar um novo par de chaves RSA
using (RSA rsa = RSA.Create())
{
    // Exportar chave pública e privada
    string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
    string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

    Console.WriteLine("Chave Pública: " + publicKey);
    Console.WriteLine("Chave Privada: " + privateKey);
}
