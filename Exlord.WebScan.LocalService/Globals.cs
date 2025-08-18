using Microsoft.Win32;
using NAPS2.Images;
using NAPS2.Scan;
using NTwain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exlord.WebScan.LocalService
{
    public static class Globals
    {
        public static HostApplicationBuilder Builder = Host.CreateApplicationBuilder(Environment.GetCommandLineArgs());
        public static SelectDeviceOptions SelectedDevice = null;

        public static SelectDeviceOptions GetSelectedScanner()
        {
            if (SelectedDevice != null) return SelectedDevice;

            SelectedDevice = new SelectDeviceOptions();
            RegistryKey key = Registry.LocalMachine.OpenSubKey($"Software\\{Consts.RegKey}", true);
            var driverName = key.GetValue(Consts.RegKeyDeviceDriver, null).ToString();
            Enum.TryParse(driverName, out Driver driver);
            var deviceId = key.GetValue(Consts.RegKeyDeviceId, null).ToString();

            if (deviceId != null)
                SelectedDevice.device = new ScannerDevice() { ID = deviceId, Driver = driver };

            Enum.TryParse(key.GetValue(Consts.RegKeyPaperSource, PaperSource.Auto).ToString(), out PaperSource paperSource);
            Enum.TryParse(key.GetValue(Consts.RegKeyBitDepth, BitDepth.Color).ToString(), out BitDepth bitDepth);

            SelectedDevice.paperSource = paperSource;
            SelectedDevice.dpi = int.Parse(key.GetValue(Consts.RegKeyDPI, 300).ToString());
            SelectedDevice.bitDepth = bitDepth;

            return SelectedDevice;
        }

    }
}
