using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using System;
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

            CreateHostBuilder(args).Build().Run();
        }

        private static void RemoteDataUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            var start = DateTime.Now;
            TeamRetrieval.GetTeams();
            var end = DateTime.Now;

            if (_remoteDataUpdate == null)
            {
                _remoteDataUpdate = new Timer(end.Subtract(start).TotalMilliseconds + 1800000);
                _remoteDataUpdate.Elapsed += RemoteDataUpdate_Elapsed;
                _remoteDataUpdate.Start();
            }

            _remoteDataUpdate.Stop();
            _remoteDataUpdate = new Timer(end.Subtract(start).TotalMilliseconds + 1800000);
            _remoteDataUpdate.Start();
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