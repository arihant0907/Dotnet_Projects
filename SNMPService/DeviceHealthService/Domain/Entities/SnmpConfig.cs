using Lextm.SharpSnmpLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceHealthService.Domain.Entities
{
    public class SnmpConfig
    {
        public int? Port { get; set; }
        public string? Community { get; set; }
        public VersionCode? Version { get; set; }
        public List<string>? Oids { get; set; }
    }
}
