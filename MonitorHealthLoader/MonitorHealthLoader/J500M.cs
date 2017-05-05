using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorHealthLoader
{
    class J500M
    {
        public String firmVersion, bootVersion, idVersion;
        private static AdbSocket mAdbSocket;
        private static DeviceData mDevice;
        private DateTime dt = new DateTime();
        Form1 mform;

        const String FIRMWARE_VERSION = "J500MUBU1AOL1", BOOTLOADER_VERSION = "J500MUBU1AOL1", ID_VERSION = "LMY48B.J500MUBU1AOL1";

        public J500M(DeviceData device, AdbSocket adbSocket, Form1 form)
        {

            mDevice = device;
            mAdbSocket = adbSocket;

            mform = form;

            if (checkDeviceInfo())
                startProcess();
            else
                form.Log("Error running " + device.Name.ToString());

        }

        private void startProcess()
        {
            //Start New Thread Hereish;
        }

        private bool checkDeviceInfo()
        {
            var receiver = new ConsoleOutputReceiver();

            firmVersion = getProperty(mDevice, "ro.build.version.incremental");
            bootVersion = getProperty(mDevice, "ro.bootloader");
            idVersion = getProperty(mDevice, "ro.build.display.id");

            mform.Log("Firmware: " + firmVersion);
            mform.Log("Bootloader: " + bootVersion);
            mform.Log("ID: " + idVersion);

            if (fmtString(firmVersion) != FIRMWARE_VERSION || fmtString(bootVersion) != BOOTLOADER_VERSION || fmtString(idVersion) != ID_VERSION)
                return false;
            else
                return true;
        }

        private string fmtString(string input)
        {
            return Regex.Replace(input, @"\r\n?|\n", "");
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

        void sendCommand(string command)
        {
            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();

            AdbClient.Instance.ExecuteRemoteCommand(command, device, receiver);

            Console.WriteLine("The device responded:");
            Console.WriteLine(receiver.ToString());
            mform.Log(receiver.ToString());
        }

    }
}
