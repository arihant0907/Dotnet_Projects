using Lextm.SharpSnmpLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceHealthService.Domain.Entities
{
    public class SnmpDevice
    {
        public string IpAddress { get; set; }
        public int Port { get; set; } = 161;
        public string Community { get; set; } = "public";
        public VersionCode Version { get; set; } = VersionCode.V2;
        public List<string> Oids { get; set; }
    }
}
