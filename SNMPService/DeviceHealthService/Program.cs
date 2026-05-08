using DeviceHealthService.Domain.Entities;
using DeviceHealthService.Domain.Interfaces;
using DeviceHealthService.Infrastructure.Repositories;
using DeviceHealthService.Infrastructure.Snmp;

namespace DeviceHealthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();

            builder.Services.Configure<DatabaseSettings>(
            builder.Configuration.GetSection("DatabaseSettings"));

            builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
            builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

            builder.Services.AddScoped<SNMPService>();

            var host = builder.Build();
            host.Run();
        }
    }
}