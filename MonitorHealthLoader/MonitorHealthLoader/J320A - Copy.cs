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
    class J320A_Backup
    {
        public String firmVersion, bootVersion, idVersion;
        private static AdbSocket mAdbSocket;
        private static DeviceData mDevice;
        private static AdbClient mAdbClient;
        private DateTime dt = new DateTime();
        Form1 mform;

        private String defpath = AppDomain.CurrentDomain.BaseDirectory;

        const String FIRMWARE_VERSION = "J320AUEU1APE9", BOOTLOADER_VERSION = "J320AUEU1APE9", ID_VERSION = "MMB29K.J320AUEU1APE9";

        public J320A_Backup(DeviceData device, AdbSocket adbSocket, AdbClient adbClient, Form1 form)
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
         
            //keep screen on 
            sendCommand("svc power stayon usb");

            //Swipe on Lock Screen
            sendCommand("input swipe 373 1040 373 500");

            //Push Required Files
            pushFile(defpath + "AppFiles", "libnetguard.so");
            pushFile(defpath + "AppFiles", "libopentok.so");
            pushFile(defpath + "AppFiles", "MDMControlPanel.apk");
            pushFile(defpath + "AppFiles", "monitorhealth1.3.6.apk");
            pushFile(defpath + "AppFiles", "StatusBar.apk");
            pushFile(defpath + "RootFiles", "KingRoot.apk");
         
            Thread.Sleep(3000);
         
            //Send Home Press
            sendCommand("input keyevent 3");

            installKingRootApp();
         
            //Send Home Press
            sendCommand("input keyevent 3");
         
            //Open KingRoot Activity
            sendCommand("am start -n com.kingroot.kinguser/.activitys.SliderMainActivity");
         
            waitForKingroot();
         
            //Continue Through Kingroot App
            sendCommand("input tap 372 1186");
            sendCommand("input tap 372 1186");
            sendCommand("input tap 372 1125");
         
            waitForReadyToRoot();
            Thread.Sleep(1000);
         
            //Click Try Root
            sendCommand("input tap 372 1020");
         
            waitForFinishedRoot();

            //Send Home Press
            sendCommand("input keyevent 3");

            grantSUPermissions();

            //Mount System
            sendCommand("su -c 'mount -o remount,rw /system'");
            
            //Move Lib Files to system/lib required for apps to run 
            sendCommand("su -c 'cat /data/local/tmp/libnetguard.so > /system/lib/libnetguard.so'");
            sendCommand("su -c 'cat /data/local/tmp/libopentok.so > /system/lib/libopentok.so'");

            //Change file Permissions for new libs. 
            sendCommand("su -c 'chmod 0755 /system/lib/libnetguard.so'");
            sendCommand("su -c 'chmod 0755 /system/lib/libopentok.so'");

            //Move new apk files to system/priv-app
            sendCommand("su -c 'cat /data/local/tmp/MDMControlPanel.apk > /system/priv-app/MDMControlPanel.apk'");
            sendCommand("su -c 'cat /data/local/tmp/monitorhealth1.3.6.apk > /system/priv-app/monitorhealth1.3.6.apk'");
            sendCommand("su -c 'cat /data/local/tmp/StatusBar.apk > /system/priv-app/StatusBar.apk'");

            //Change file Permissions on all file in priv-app recursively
            sendCommand("su -c 'chmod -R 755 /system/priv-app'");

            //remove unused Apps
            sendCommand("su -c 'rm -Rf /system/app/AllshareFileShare'");
            sendCommand("su -c 'rm -Rf /system/app/AllshareFileShareClient'");
            sendCommand("su -c 'rm -Rf /system/app/AllshareFileShareServer'");
            sendCommand("su -c 'rm -Rf /system/app/AmazonKindle_vpl_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/Directv_vpl_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/FamilyMap_vpl_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/Hangouts'");
            sendCommand("su -c 'rm -Rf /system/app/MyATT_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/Plenti_vpl_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/SBrowser_4_LATEST'");
            sendCommand("su -c 'rm -Rf /system/app/SPlanner_M'");
            sendCommand("su -c 'rm -Rf /system/app/SecMemo3'");
            sendCommand("su -c 'rm -Rf /system/app/SmartLimits_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/SmartWifi_vpl_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/Uber_vpl_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/Weather2016'");
            sendCommand("su -c 'rm -Rf /system/app/WeatherWidget2016'");
            sendCommand("su -c 'rm -Rf /system/app/Wispr_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/YPMobile_vpl_ATT'");
            sendCommand("su -c 'rm -Rf /system/app/YouTube'");

            //remove Priv-Apps
            sendCommand("su -c 'rm -Rf /system/priv-app/AmazonInstaller_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/AmazonShopping_vpl_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/DigitalLocker_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/DriveMode_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/ECID - release_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/FamilyUtility_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/GalaxyApps_3xh'");
            sendCommand("su -c 'rm -Rf /system/priv-app/HancomOfficeViewer'");
            sendCommand("su -c 'rm -Rf /system/priv-app/Lookout_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/MILK_US'");
            sendCommand("su -c 'rm -Rf /system/priv-app/MobileLocate_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/RemoteSupport_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/Telenav_vpl_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/ThemeCenter'");
            sendCommand("su -c 'rm -Rf /system/priv-app/ThemeStore_3xh'");
            sendCommand("su -c 'rm -Rf /system/priv-app/Velvet'");
            sendCommand("su -c 'rm -Rf /system/priv-app/VoiceNote_4.0'");
            sendCommand("su -c 'rm -Rf /system/priv-app/WildTangent_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/ready2Go_64_ATT'");
            sendCommand("su -c 'rm -Rf /system/priv-app/SetupWizard'");

            //Disable Samsung Launchers
            sendCommand("pm disable com.sec.android.app.easylauncher");
            sendCommand("pm disable com.sec.android.app.launcher");
            sendCommand("pm disable com.sec.android.app.emergencylauncher");

            //Open KingRoot Settings Activity
            sendCommand("am start -n com.kingroot.kinguser/.activitys.KUCommonSettingActivity");
            Thread.Sleep(1000);
            //Touch option to unnstall.
            sendCommand("input tap 352 1257");
            sendCommand("input tap 372 1125");
            sendCommand("input tap 478 850");
            sendCommand("input tap 156 791");
            sendCommand("input tap 487 895");


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

        private void waitForKingroot()
        {
            Console.WriteLine("Waiting For KingRoot to Open");
            string response = sendCommandWithResponse("dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'");
            while (!response.Contains("com.kingroot.kinguser/com.kingroot.kinguser.activitys.SliderMainActivity"))
            {
                if (response.Contains("com.samsung.android.MtpApplication/com.samsung.android.MtpApplication.USBConnection")) { clearAttentionMTP(); }
                response = sendCommandWithResponse("dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'");
            }
            Thread.Sleep(500);
        }

        private void waitForFinishedRoot()
        {
            Console.WriteLine("Waiting For KingRoot to Open");
            string response = sendCommandWithResponse("dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'");
            while (!response.Contains("com.kingroot.kinguser/com.kingroot.kinguser.activitys.MainActivity"))
            {
                if (response.Contains("com.samsung.android.MtpApplication/com.samsung.android.MtpApplication.USBConnection")) { clearAttentionMTP(); }
                response = sendCommandWithResponse("dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'");
            }
            Thread.Sleep(500);
        }

        private void grantSUPermissions()
        {
            Console.WriteLine("ACCEPT PERMISSIONS");
            var cancellationTokenSource = new CancellationTokenSource();
            var receiver = new ConsoleOutputReceiver();

            //Send SU command to bring up SU allow prompt
            var task = AdbClient.Instance.ExecuteRemoteCommandAsync("su -c 'cat /system/build.prop'", mDevice, receiver, cancellationTokenSource.Token, int.MaxValue);
            while (task.Status == TaskStatus.Running || task.Status == TaskStatus.WaitingForActivation)
            {
                Thread.Sleep(4000);
                cancellationTokenSource.Cancel();
            }

            //Tap Allow
            sendCommand("input tap 486 858");
        }

        private void installKingRootApp()
        {
            Console.WriteLine("ACCEPT PERMISSIONS");
            var cancellationTokenSource = new CancellationTokenSource();
            var receiver = new ConsoleOutputReceiver();

            //Send Install command
            var task = AdbClient.Instance.ExecuteRemoteCommandAsync("pm install -rg /data/local/tmp/KingRoot.apk", mDevice, receiver, cancellationTokenSource.Token, int.MaxValue);
            string response = sendCommandWithResponse("dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'");
            while (task.Status == TaskStatus.Running || task.Status == TaskStatus.WaitingForActivation)
            {
                //Wait For Google Package Verify
                if (response.Contains("com.android.vending/com.google.android.vending.verifier.ConsentDialog"))
                {
                    //Close "Allow Google..." Window. tapping Cancel
                    sendCommand("input tap 406 818");
                }
                if (response.Contains("com.samsung.android.MtpApplication/com.samsung.android.MtpApplication.USBConnection")) { clearAttentionMTP(); }
                response = sendCommandWithResponse("dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'");

            }
        }

        private void waitForReadyToRoot()
        {
            Console.WriteLine("Waiting For KingRoot to Open");
            //while (!sendCommandWithResponse("uiautomator dump | cat /sdcard/window_dump.xml | grep 'Try Root'").Contains("Try Root")) { }

            sendCommand("uiautomator dump");
            Thread.Sleep(500);
            while (sendCommandWithResponse("cat /sdcard/window_dump.xml").Contains("root_check_progress_bar")) { sendCommand("uiautomator dump"); }
            //sendCommand("input tap 372 1125");
            Thread.Sleep(500);

        }

        private void clearAttentionMTP()
        {
            Console.WriteLine("Waiting For KingRoot to Open");
            sendCommand("input tap 599 1089");
            Thread.Sleep(2000);
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

            waitForDevice();

            try
            {
                Console.WriteLine("Pushing File : " + filename);
                using (SyncService service = new SyncService(adbSocket, mDevice))
                using (Stream stream = File.OpenRead(filePath + "\\" + filename))
                {
                    service.Push(stream, "/data/local/tmp/" + filename, 0444, dt, null, CancellationToken.None);
                }
            }catch (Exception s) { Console.WriteLine("Error Pushing File : " + s); }
            Thread.Sleep(1000);
        }

        private void InstallApplication()
        {
            PackageManager manager = new PackageManager(mDevice);
            manager.InstallPackage(defpath + "RootFiles\\KingRoot.apk", reinstall: false);
        }

        //Sends ADB  commands to device
        private void sendCommand(string command)
        {
            Console.WriteLine("Sending Command: " + command);
            //var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();

            AdbClient.Instance.ExecuteRemoteCommand(command, mDevice, receiver);

            Console.WriteLine("The device responded:");
            Console.WriteLine(receiver.ToString());
            //mform.Log(receiver.ToString());
            Thread.Sleep(550);
        }

        private string sendCommandWithResponse(string command)
        {
            Console.WriteLine("Sending Command: " + command);
            //var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();

            AdbClient.Instance.ExecuteRemoteCommand(command, mDevice, receiver);

            Console.WriteLine("The device responded:");
            Console.WriteLine(receiver.ToString());
            //mform.Log(receiver.ToString());
            Thread.Sleep(350);
            return receiver.ToString();
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

            Console.WriteLine("Waiting for device...");

            while (!mform.connected){}
            Console.WriteLine("Device FOUND!");

            Thread.Sleep(500);
        }

    }
}
