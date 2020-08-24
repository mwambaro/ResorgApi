using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Linq;

// DevOps Credentials:
// Username: onkezabahizi
// password: yht7xafgry3gcgfq3dkpuvu4vjzb7esvnrwfjnpv32mc2ssvsp4a

namespace ResorgApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseStartup<Startup>()
                    .UseKestrel(options => options.ConfigureEndpoints());
                });

    }

    public static class KestrelServerOptionsExtensions
    {
        public static void ConfigureEndpoints(this KestrelServerOptions options)
        {
            var configuration = (IConfiguration)options.ApplicationServices.GetService(typeof(IConfiguration));
            var environment = (IHostEnvironment)options.ApplicationServices.GetService(typeof(IHostEnvironment));

            var endpoints = configuration.GetSection("HttpServer:Endpoints")
                .GetChildren()
                .ToDictionary(section => section.Key, section =>
                {
                    var endpoint = new EndpointConfiguration();
                    section.Bind(endpoint);
                    return endpoint;
                });

            foreach (var endpoint in endpoints)
            {
                var config = endpoint.Value;
                var port = config.Port ?? (config.Scheme == "https" ? 443 : 80);

                var ipAddresses = new List<IPAddress>();
                if (config.Host == "localhost")
                {
                    ipAddresses.Add(IPAddress.IPv6Loopback);
                    ipAddresses.Add(IPAddress.Loopback);
                }
                else if (IPAddress.TryParse(config.Host, out var address))
                {
                    ipAddresses.Add(address);
                }
                else
                {
                    ipAddresses.Add(IPAddress.IPv6Any);
                }

                foreach (var address in ipAddresses)
                {
                    options.Listen(address, port,
                        listenOptions =>
                        {
                            if (config.Scheme == "https")
                            {
                                var certificate = LoadCertificate(environment, config);
                                listenOptions.UseHttps(certificate);
                            }
                        });
                }
            }
        }

        public static X509Certificate2 LoadCertificate(IHostEnvironment env=default, EndpointConfiguration cconfig =default)
        {
            X509Certificate2 cert = default;
            
            try
            {
                bool isDev = true == env?.IsDevelopment();
                if (default == cconfig && isDev)
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .AddJsonFile("certificate.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"certificate.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                        .Build();

                    var certificateSettings = config.GetSection("certificateSettings");
                    string certificateFileName = certificateSettings.GetValue<string>("fileName");
                    string password = certificateSettings.GetValue<string>("password");
                    string certificatePassword = DecodeSecureString(password);

                    System.Diagnostics.Debug.WriteLine($"Certificate: {certificateFileName}; Password: {certificatePassword}");

                    var certificate = new X509Certificate2(certificateFileName, certificatePassword);

                    cert = certificate;
                }
                else
                {
                    if (cconfig?.StoreName != null && cconfig?.StoreLocation != null)
                    {
                        using (var store = new X509Store(cconfig.StoreName, Enum.Parse<StoreLocation>(cconfig.StoreLocation)))
                        {
                            
                            store.Open(OpenFlags.ReadOnly);
                            var certificate = store.Certificates.Find(
                                X509FindType.FindBySubjectName,
                                cconfig.Host,
                                validOnly: true
                            );

                            if (certificate.Count > 0)
                            {
                                cert = certificate[0];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return cert;
        }

        /// <summary>
        /// Decodes a secure string
        /// </summary>
        /// <param name="s">A SecureString or a Stringified SecureString object.</param>
        /// <returns>a decoded form of the secure string</returns>
        public static string DecodeSecureString(dynamic s)
        {
            string usstring = null;

            try
            {
                IntPtr bstr = default;
                if (s is SecureString)
                    bstr = Marshal.SecureStringToBSTR(s);
                else if (s is string)
                    bstr = Marshal.StringToBSTR(s);

                if (default != bstr)
                {
                    usstring = Marshal.PtrToStringBSTR(bstr);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return usstring;
        }
    }

    public class EndpointConfiguration
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Scheme { get; set; }
        public string StoreName { get; set; }
        public string StoreLocation { get; set; }
        public string FilePath { get; set; }
        public string Password { get; set; }
    }
}
