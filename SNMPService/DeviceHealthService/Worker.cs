using DeviceHealthService.Domain.Entities;
using DeviceHealthService.Domain.Interfaces;
using DeviceHealthService.Infrastructure.Snmp;
using Lextm.SharpSnmpLib;
using System.Text.Json;

namespace DeviceHealthService
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var repository = scope.ServiceProvider
                        .GetRequiredService<IDeviceRepository>();

                    var snmpService = scope.ServiceProvider
                        .GetRequiredService<SNMPService>();

                    var devices = await repository.GetDevicesAsync();

                    var allMetrics = new List<DeviceMetric>();

                    foreach (var device in devices)
                    {
                        try
                        {
                            var snmpDevice = BuildSnmpDevice(device);

                            var snmpResult = await snmpService.GetAsync(snmpDevice);

                            Console.WriteLine("snmp result", snmpResult);

                            var metrics = MapToMetrics(device, snmpResult);

                            allMetrics.AddRange(metrics);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"SNMP failed for device {device.Id}");
                        }
                    }

                    if (allMetrics.Any())
                    {
                        await repository.AddMetricsBulkAsync(allMetrics);
                        _logger.LogInformation($"Inserted {allMetrics.Count} metrics");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker loop failed");
                }

                await Task.Delay(TimeSpan.FromSeconds(5000), stoppingToken);
            }
        }

        private List<DeviceMetric> MapToMetrics(Device device, List<Lextm.SharpSnmpLib.Variable> variables)
        {
            var metrics = new List<DeviceMetric>();

            foreach (var v in variables)
            {
                metrics.Add(new DeviceMetric
                {
                    DeviceId = device.Id,
                    MetricName = GetMetricName(v.Id.ToString()),
                    MetricValue = v.Data.ToString(),
                    Timestamp = DateTime.UtcNow
                });
            }

            return metrics;
        }

        private string GetMetricName(string oid)
        {
            return oid switch
            {
                "1.3.6.1.2.1.1.5.0" => "SysName",
                "1.3.6.1.2.1.1.6.0" => "SysLocation",
                "1.3.6.1.2.1.25.3.3.1.2.1" => "CPU",
                "1.3.6.1.2.1.25.2.3.1.6.1" => "Memory",
                _ => oid // fallback
            };
        }

        private SnmpDevice BuildSnmpDevice(Device device)
        {
            var snmpDevice = new SnmpDevice
            {
                IpAddress = device.IpAddress
            };

            if (!string.IsNullOrEmpty(device.PropertiesJson))
            {
                var config = JsonSerializer.Deserialize<SnmpConfig>(device.PropertiesJson);

                if (config != null)
                {
                    snmpDevice.Port = config.Port ?? 161;
                    snmpDevice.Community = config.Community ?? "public";
                    snmpDevice.Version = config.Version ?? VersionCode.V2;
                    snmpDevice.Oids = config.Oids ?? new List<string>();
                }
            }

            return snmpDevice;
        }
    }
}