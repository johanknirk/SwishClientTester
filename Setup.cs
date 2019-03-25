using System.Security.Cryptography.X509Certificates;

namespace SwishClientTester
{
    public class Setup
    {
        public static void AddCertificatesToStore(string path)
        {
            // Client
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);                        
            store.Add(new X509Certificate2($"{path}/1231181189.p12", "swish"));            
            store.Close();
            
            store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            // CN = Swedbank Customer CA1 v1 for Swish
            //store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile($"{path}/swedbank_cust_ca1.cer")));
            // CN = Swedbank Root CA v2 for Swish Test
            //store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile($"{path}/swedbank_root_ca.cer")));
            store.Close();

            // CN = Swish Root CA v2 Test
            store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile($"{path}/swedbank_cust_ca1.cer")));
            store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile($"{path}/swedbank_root_ca.cer")));
            store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile($"{path}/swish_root_ca_v2_test.cer")));
            store.Close();
        }
    }
}