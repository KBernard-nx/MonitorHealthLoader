using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TestStack.White.UIItems.WindowItems;

namespace MonitorHealthLoader
{
    class J320A
    {
        public String firmVersion, bootVersion, idVersion;
        private static AdbSocket mAdbSocket;
        private static DeviceData mDevice;
        private static AdbClient mAdbClient;
        private DateTime dt = new DateTime();
        Form1 mform;

        private String defpath = AppDomain.CurrentDomain.BaseDirectory;

        const String FIRMWARE_VERSION = "J320AUEU1APE9", BOOTLOADER_VERSION = "J320AUEU1APE9", ID_VERSION = "MMB29K.J320AUEU1APE9";

        public J320A(DeviceData device, AdbSocket adbSocket, AdbClient adbClient, Form1 form)
        {

            mDevice = device;
            mAdbSocket = adbSocket;
            mAdbClient = adbClient;
            mform = form;

        }

        public void startProcess()
        {

            if (!checkDeviceInfo())
                return;

            //init Odin Bootloader flash
            flashboot();

            //WaitForDevice
            waitForDevice();

            //Push Required Files
            pushFile(defpath + "AppFiles",  "libnetguard.so");
            pushFile(defpath + "AppFiles",  "libopentok.so");
            pushFile(defpath + "AppFiles",  "MDMControlPanel.apk");
            pushFile(defpath + "AppFiles",  "monitorhealth1.3.6.apk");
            pushFile(defpath + "AppFiles",  "StatusBar.apk");
            //pushFile(defpath + "RootFiles",  "KingRoot.apk");
            InstallApplication();


        }

        private bool checkDeviceInfo()
        {
            var receiver = new ConsoleOutputReceiver();

            firmVersion = getProperty("ro.build.version.incremental");
            bootVersion = getProperty("ro.bootloader");
            idVersion = getProperty("ro.build.display.id");

            mform.Log("Firmware: " + firmVersion);
            mform.Log("Bootloader: " + bootVersion);
            mform.Log("ID: " + idVersion);

            if (fmtString(firmVersion) != FIRMWARE_VERSION || fmtString(bootVersion) != BOOTLOADER_VERSION || fmtString(idVersion) != ID_VERSION)
                return false;
            else
                return true;
        }


        //===========================================================
        //Odin Flashing Boot Img For Root
        //===========================================================

        //Automate Odin
        private void flashboot()
        {
            //Reboot device into Download mode
            var receiver = new ConsoleOutputReceiver();

            //Reboot to Download Mode
            sendCommand("reboot download");

            //Waits til the computer sees the device in Download Mode
            waitForDownloadMode();

            try
            {
                //Open Odin to flash Boot.img needed for root.
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "Odin/Odin3_v3.11.1.exe";
                processInfo.WorkingDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory + "Odin/");
                processInfo.ErrorDialog = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;
                Process process = Process.Start(processInfo);

                //Attach to Odin process
                TestStack.White.Application application = TestStack.White.Application.Attach(process);

                Thread.Sleep(1000);

                //Get Odin Window
                Window window = application.GetWindow("Odin3 v3.11", TestStack.White.Factory.InitializeOption.NoCache);

                //Click Bootloader Button
                TestStack.White.UIItems.Button btnBootloader = window.Get<TestStack.White.UIItems.Button>("BL");
                btnBootloader.Click();

                //Get OpenFileDialog as child.
                List<Window> modalWindows = window.ModalWindows(); //list of all the modal windows belong to the window.
                Window childWindow = window.ModalWindow("Open"); //modal window with title "child"
                if (childWindow.IsModal)
                {
                    //Handle OpenFileDialog, Navigate to file and select it.
                    TestStack.White.UIItems.ListBoxItems.ComboBox filePaths;
                    filePaths = childWindow.Get<TestStack.White.UIItems.ListBoxItems.ComboBox>(TestStack.White.UIItems.Finders.SearchCriteria.ByAutomationId("1148"));
                    filePaths.EditableText = AppDomain.CurrentDomain.BaseDirectory + "Odin\\j320root_boot.tar";
                    TestStack.White.UIItems.Button openBtn = childWindow.Get<TestStack.White.UIItems.Button>("Open");
                    openBtn.Click();
                }

                //Start the Flash 
                TestStack.White.UIItems.Button btnFlash = window.Get<TestStack.White.UIItems.Button>("Start");
                btnFlash.Click();

                //Check for Download to finish
                TestStack.White.UIItems.Label odinCheck;
                odinCheck = window.Get<TestStack.White.UIItems.Label>(TestStack.White.UIItems.Finders.SearchCriteria.ByAutomationId("1062"));

                //Loop til Pass! or Fail
                while (odinCheck.Text != "PASS!" || odinCheck.Text.Contains("FAIL"))
                {
                    odinCheck = window.Get<TestStack.White.UIItems.Label>(TestStack.White.UIItems.Finders.SearchCriteria.ByAutomationId("1062"));
                }

                //Close Odin
                application.Close();

            }
            catch (Exception es)
            {
                Console.WriteLine("Error Running Odin!\n" + es);
            }
        }

        //Waits for the Samsung Download Mode Drivers to appear. 
        private void waitForDownloadMode()
        {
            ManagementObjectSearcher objSearcher = new ManagementObjectSearcher("Select * from Win32_PNPEntity");
            //TODO: set a time limit on this. then install Drivers.
            while (true)
            {

                ManagementObjectCollection objCollection = objSearcher.Get();

                foreach (ManagementObject obj in objCollection)
                {

                    string info = String.Format("Device='{0}'", obj["Description"]);
                    if (info.Contains("SAMSUNG Mobile USB CDC Composite Device")) { return; }
                }

                Console.WriteLine("Device in downlaod mode Not Found");
            }
        }

        //=================================================================
        //                      Helper Methods 
        //=================================================================

        //Pushes a File to the Device
        private void pushFile(string filePath, string filename)
        {
            AdbSocket adbSocket = new AdbSocket(AdbClient.Instance.EndPoint);

            try
            {
                Console.WriteLine("Pushing File : " + filename);
                using (SyncService service = new SyncService(adbSocket, mDevice))
                using (Stream stream = File.OpenRead(filePath + "\\" + filename))
                {
                    service.Push(stream, "/data/local/tmp/" + filename, 0444, dt, null, CancellationToken.None);
                }
            }catch (Exception s) { Console.WriteLine("Error Pushing File : " + s); }
        }

        private void InstallApplication()
        {
            PackageManager manager = new PackageManager(mDevice);
            manager.InstallPackage(defpath + "RootFiles\\KingRoot.apk", reinstall: false);
        }

        //Sends ADB  commands to device
        private void sendCommand(string command)
        {
            //var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();

            AdbClient.Instance.ExecuteRemoteCommand(command, mDevice, receiver);

            Console.WriteLine("The device responded:");
            Console.WriteLine(receiver.ToString());
            //mform.Log(receiver.ToString());
        }

        //formats strings, Removes newline and enter chars
        private string fmtString(string input)
        {
            return Regex.Replace(input, @"\r\n?|\n", "");
        }

        //Gets Properties from the device, used to check firmware version.
        private String getProperty(string property)
        {
            var receiver = new ConsoleOutputReceiver();

            AdbClient.Instance.ExecuteRemoteCommand("getprop " + property, mDevice, receiver);
            return receiver.ToString();
        }

        private void waitForDevice()
        {
            bool clearedForTakeOff = false;

            Console.WriteLine("Waiting for device!");
            List<DeviceData> ds = AdbClient.Instance.GetDevices();

            while (!clearedForTakeOff)
            {
                
                foreach (DeviceData d in ds)
                 {
                     if(d.State == DeviceState.Online)
                    {
                        clearedForTakeOff = true;
                        Console.WriteLine("Device Connected!");
                    }
                 }
           
                ds = AdbClient.Instance.GetDevices();
            }
            Console.WriteLine("Device FOUND!");

        }

    }
}
