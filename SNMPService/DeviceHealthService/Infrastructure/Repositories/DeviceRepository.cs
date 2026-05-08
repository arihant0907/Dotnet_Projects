using Dapper;
using DeviceHealthService.Domain.Entities;
using DeviceHealthService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceHealthService.Infrastructure.Repositories
{
    public class DeviceRepository:IDeviceRepository
    {
        private readonly IDbConnectionFactory _factory;

        public DeviceRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<Device>> GetDevicesAsync()
        {
            using var connection = await _factory.CreateConnectionAsync();


            var sql = "SELECT * FROM Device";
            return await connection.QueryAsync<Device>(sql);
        }

        public async Task<IEnumerable<DeviceMetric>> GetLatestMetricsAsync(string deviceId)
        {
            using var connection = await _factory.CreateConnectionAsync();

            var sql = @"
                SELECT dm.*
                FROM DeviceMetrics dm
                INNER JOIN (
                    SELECT MetricName, MAX(Timestamp) AS MaxTime
                    FROM DeviceMetrics
                    WHERE DeviceId = @DeviceId
                    GROUP BY MetricName
                ) latest
                ON dm.MetricName = latest.MetricName
                AND dm.Timestamp = latest.MaxTime
                WHERE dm.DeviceId = @DeviceId;
            ";

            return await connection.QueryAsync<DeviceMetric>(sql, new { DeviceId = deviceId });
        }

        public async Task<int> AddMetricsBulkAsync(IEnumerable<DeviceMetric> metrics)
        {
            using var connection = await _factory.CreateConnectionAsync();

            var sql = @"
                INSERT INTO DeviceMetrics (DeviceId, MetricName, MetricValue, Timestamp)
                VALUES (@DeviceId, @MetricName, @MetricValue, @Timestamp);
            ";

            return await connection.ExecuteAsync(sql, metrics);
        }
    }
}
