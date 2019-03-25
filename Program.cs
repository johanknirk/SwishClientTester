using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SwishClientTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Setup certificates in container 
            Setup.AddCertificatesToStore($"{currentDir}{Path.DirectorySeparatorChar}certificates");

            X509Certificate2 clientCertificate = null;
            // CN = 1234679304 O = 5560997982 C = SE
            var clientThumbPrint = "1AE44567D24B2122661C5337815C7C38949D6FF3";
            using (var certStore = new X509Store(StoreLocation.CurrentUser))
            {
                certStore.Open(OpenFlags.ReadOnly);
                var certs = certStore.Certificates.Find(X509FindType.FindByThumbprint, clientThumbPrint.ToUpperInvariant(), false);
                clientCertificate = certs[0];
                certStore.Close();
            }        

            var client = new SwishClient();
            var req = new SwishPaymentRequest
            {
                PayeeAlias = "1231181189",
                PayeePaymentReference = "0123456789",
                Amount = "100",
                Currency = "SEK",
                CallbackUrl = "https://myfakehost.se/swishcallback.cfm",
                Message = "Kingston USB Flash Drive 8 GB"
            };

            var res = client.CreatePaymentRequest(req, clientCertificate).Result;
            Console.WriteLine("Result : " + res.IsSuccess);
            Console.WriteLine("TransactionId : " + res.TransactionId);
        }
    }
}
