using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceHealthService.Domain.Entities
{
    public class DeviceMetric
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }

        public string MetricName { get; set; }  
        public string MetricValue { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
