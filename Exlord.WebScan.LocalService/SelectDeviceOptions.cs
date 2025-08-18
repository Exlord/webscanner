using NAPS2.Images;
using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exlord.WebScan.LocalService
{
    public class SelectDeviceOptions
    {
        public ScannerDevice device { get; set; }
        public PaperSource paperSource { get; set; } = PaperSource.Auto;
        public BitDepth bitDepth { get; set; } = BitDepth.Color;
        public int dpi { get; set; } = 300;
    }
}
