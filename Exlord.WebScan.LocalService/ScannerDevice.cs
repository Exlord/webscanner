using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exlord.WebScan.LocalService
{
    public class ScannerDevice
    {
        public string Name { get; set; } = null;
        public string ID { get; set; } = null;
        public Driver Driver { get; set; } = 0;
    }
}
