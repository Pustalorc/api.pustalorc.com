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
        private static Timer _remoteDataUpdate;

        public static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(l => RemoteDataUpdate_Elapsed(null, null));

            _remoteDataUpdate = new Timer(14400000);
            _remoteDataUpdate.Elapsed += RemoteDataUpdate_Elapsed;
            _remoteDataUpdate.Start();

            CreateHostBuilder(args).Build().Run();
        }

        private static void RemoteDataUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            RainbowSixTeams.RetrieveGroups();
            LeagueOfLegendsTeams.RetrieveGroups();
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