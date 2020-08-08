using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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
                    webBuilder.UseStartup<Startup>()
                        .UseUrls("http://localhost:4000");
                });
    }
}
