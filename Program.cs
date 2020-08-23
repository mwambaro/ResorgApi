using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

using System;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;

// DevOps Credentials:
// Username: onkezabahizi
// password: yht7xafgry3gcgfq3dkpuvu4vjzb7esvnrwfjnpv32mc2ssvsp4a

namespace ResorgApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Run();
        }

        public static IWebHost CreateHostBuilder(string[] args)
        {
            IWebHost host = default;
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables()
                    .AddJsonFile("certificate.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"certificate.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                    .Build();

                    var certificateSettings = config.GetSection("certificateSettings");
                    string certificateFileName = certificateSettings.GetValue<string>("filename");
                    string certificatePassword = new SecureString..(certificateSettings.GetValue<string>("password"));

                    var certificate = new X509Certificate2(certificateFileName, certificatePassword);

                host = new WebHostBuilder()
                    .UseKestrel(
                        options =>
                        {
                            options.AddServerHeader = false;
                            options.Listen(IPAddress.Loopback, 44321, listenOptions =>
                            {
                                listenOptions.UseHttps(certificate);
                            });
                        }
                    )
                    .UseConfiguration(config)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .UseUrls("https://localhost:44321")
                    .Build();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return host;
        }



        private string ConvertFromSecureString(SecureString sstring)
        {
            string usstring = null;

            try
            {
                var bstr = Marshal.SecureStringToBSTR(sstring);
                usstring = Marshal.PtrToStringBSTR(bstr);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return usstring;
        }
    }
}
