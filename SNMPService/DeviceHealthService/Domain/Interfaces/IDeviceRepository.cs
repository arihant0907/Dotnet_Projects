using DeviceHealthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceHealthService.Domain.Interfaces
{
    public interface IDeviceRepository
    {
         public Task<IEnumerable<Device>> GetDevicesAsync();

        public  Task<IEnumerable<DeviceMetric>> GetLatestMetricsAsync(string deviceId);

        public Task<int> AddMetricsBulkAsync(IEnumerable<DeviceMetric> metrics);
    }
}
