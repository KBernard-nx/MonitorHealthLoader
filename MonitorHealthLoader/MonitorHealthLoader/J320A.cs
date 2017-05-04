using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorHealthLoader
{
    class J320A
    {
        public String firmVersion, bootVersion, idVersion;
        private static AdbSocket mAdbSocket;
        private static DeviceData mDevice;
        private DateTime dt = new DateTime();
        Form1 mform;

        public J320A(DeviceData device, AdbSocket adbSocket, Form1 form)
        {

            mDevice = device;
            mAdbSocket = adbSocket;

            mform = form;

            checkDeviceInfo();

            

        }

        private void checkDeviceInfo()
        {

            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();

            firmVersion = getProperty(device, "ro.build.version.incremental");

            bootVersion = getProperty(device, "ro.bootloader");

            idVersion = getProperty(device, "ro.build.display.id");

            mform.Log("Firmware: " + firmVersion);
            mform.Log("Bootloader: " + bootVersion);
            mform.Log("ID: " + idVersion);

        }


        private String getProperty(DeviceData device, string property)
        {
            var receiver = new ConsoleOutputReceiver();

            AdbClient.Instance.ExecuteRemoteCommand("getprop " + property, device, receiver);
            return receiver.ToString();
        }




        

        private void pushFile()
        {
            using (SyncService service = new SyncService(mAdbSocket, mDevice))
            using (Stream stream = File.OpenRead(@"G:\j320\Process_Monitor_Health\files\Kingroot.apk"))
            {
                service.Push(stream, "/data/local/tmp/Kingroot.apk", 0444, dt, null, CancellationToken.None);
            }
        }

    }
}
