using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceHealthService.Domain.Entities
{
    public class DatabaseSettings
    {
        public string Provider { get; set; }
        public Dictionary<string, string> ConnectionStrings { get; set; }
    }
}
