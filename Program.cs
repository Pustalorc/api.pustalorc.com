using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace api.pustalorc.xyz
{
    public class Program
    {
        private static Timer RemoteDataUpdate;

        public static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(l => { RainbowSixTeams.RetrieveGroups(); });

            RemoteDataUpdate = new Timer(1800000);
            RemoteDataUpdate.Elapsed += RemoteDataUpdate_Elapsed;
            RemoteDataUpdate.Start();

            CreateHostBuilder(args).Build().Run();
        }

        private static void RemoteDataUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            RainbowSixTeams.RetrieveGroups();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.ConfigureKestrel(options =>
                    {
                        options.Limits.MinRequestBodyDataRate = null;
                        options.ListenLocalhost(50052,
                            listenOptions => listenOptions.Protocols = HttpProtocols.Http1AndHttp2);
                    })
                    .UseStartup<Startup>());
        }
    }
}