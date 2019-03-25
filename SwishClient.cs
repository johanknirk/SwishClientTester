using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SwishClientTester
{
    public class Error
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string AdditionalInformation { get; set; }
    }

    public class SwishPaymentOrRefundRequestResult
    {
        public SwishPaymentOrRefundRequestResult()
        {
            ValidationErrors = new List<Error>();
        }
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public List<Error> ValidationErrors { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    public class SwishClient
    {
        public async Task<SwishPaymentOrRefundRequestResult> CreatePaymentRequest(SwishPaymentRequest request, X509Certificate2 clientCertificate)
        {
            var body = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            using (var httpClient = CreateHttpClient(clientCertificate))
            {
                var response = await httpClient.PostAsync($"https://mss.cpc.getswish.net/swish-cpcapi/api/v1/paymentrequests", new StringContent(body, Encoding.UTF8, "application/json"));

                var result = new SwishPaymentOrRefundRequestResult
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    TransactionId = GetTransactionIdFromResponse(response)
                };

                // Situations: 
                // 401/403 - Enrolment issues (client certificate or wrong swish number of Merchant)
                // 400/422 - Wrong input data (unexpected at this point)
                // 500  - Swish is failing
                if ((int)response.StatusCode == 422)
                {
                    result.ValidationErrors = JsonConvert.DeserializeObject<List<Error>>(await response.Content.ReadAsStringAsync());
                }

                return result;
            }
        }


        private string GetTransactionIdFromResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var location = response.Headers.Location;
            return location.Segments.LastOrDefault() ?? string.Empty;
        }

        public void Direct(string url, X509Certificate2 clientCertificate)
        {
            // Create a TCP/IP client socket.
            TcpClient client = new TcpClient(url, 443);
            Console.WriteLine("Client connected.");
            // Create an SSL stream that will close the client's stream.
            var sslStream = new SslStream(
                client.GetStream(),
                true,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                new LocalCertificateSelectionCallback(SelectLocalCertificate)
            );

            sslStream.AuthenticateAsClient(url, new X509Certificate2Collection(clientCertificate), false);
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        public static X509Certificate SelectLocalCertificate(
            object sender,
            string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate,
            string[] acceptableIssuers)
        {
            Console.WriteLine("Client is selecting a local certificate.");
            if (acceptableIssuers != null &&
                acceptableIssuers.Length > 0 &&
                localCertificates != null &&
                localCertificates.Count > 0)
            {
                // Use the first certificate that is from an acceptable issuer.
                foreach (X509Certificate certificate in localCertificates)
                {
                    string issuer = certificate.Issuer;
                    if (Array.IndexOf(acceptableIssuers, issuer) != -1)
                        return certificate;
                }
            }
            if (localCertificates != null &&
                localCertificates.Count > 0)
                return localCertificates[0];

            return null;
        }

        private static HttpClient CreateHttpClient(X509Certificate2 clientCertificate2)
        {
            var handler = CreateRequestHandlerWithClientCertificates(clientCertificate2);

            Console.WriteLine("clientCertificate2.HasPrivateKey: " + clientCertificate2.HasPrivateKey);

            return new HttpClient(handler);
        }

        private static HttpClientHandler CreateRequestHandlerWithClientCertificates(X509Certificate2 clientCertificate)
        {
            var handler = new HttpClientHandler();

            handler.ClientCertificates.Clear();
            handler.ClientCertificates.Add(clientCertificate);

            //handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;

            return handler;
        }
    }
}