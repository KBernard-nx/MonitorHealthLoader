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
using System.Windows.Forms;
using TestStack.White.UIItems.WindowItems;


namespace MonitorHealthLoader
{
    class J327U
    {
        public String firmVersion, bootVersion, idVersion;
        private static AdbSocket mAdbSocket;
        private static DeviceData mDevice;
        private static AdbClient mAdbClient;
        private DateTime dt = new DateTime();
        Form1 mform;
        ProgressBar mProgressBar;

        private String defpath = AppDomain.CurrentDomain.BaseDirectory;

        const String FIRMWARE_VERSION = "J327UUEU1AQE5", BOOTLOADER_VERSION = "J327UUEU1AQE5", ID_VERSION = "NRD90M.J327UUEU1AQE5";
        const String FIRMWARE_VERSION_1 = "J320AUES2APJ2", BOOTLOADER_VERSION_1 = "J320AUES2APJ2", ID_VERSION_1 = "MMB29K.J320AUES2APJ2";

        public J327U(DeviceData device, AdbSocket adbSocket, AdbClient adbClient, Form1 form, ProgressBar progressBar)
        {

            mDevice = device;
            mAdbSocket = adbSocket;
            mAdbClient = adbClient;
            mform = form;
            mProgressBar = progressBar;

        }

        public void startProcess()
        {
            updateProgress();

            if (!checkDeviceInfo())
            {
                mform.Log("ERROR: Update Device Firmware to J327UUEU1AQE5!");
                return;
            }


            mform.Log("Restarting device into Download Mode");
            //CFAutoRoot
            flashRecovery();
            updateProgress();

            //WaitForDevice
            mform.Log("Waiting For Device...");
            mform.Log("WAIT FOR VERIFICATION FAILED - THEN PRESS RESET!!!");
            mform.Log("Then Exit program and Run again"); 
            waitForDevice();

            //CFAutoRoot Again
           // flashRecovery();
           // updateProgress();

            //WaitForDevice
           // mform.Log("Waiting For Device...");
           // waitForDevice();

            //Swipe on Lock Screen
            mform.Log("Opening Lock Screen");
            Thread.Sleep(500);
            sendCommand("input swipe 373 1040 373 500");

            Thread.Sleep(500);

            grantSUPermissions();

            Thread.Sleep(500);


            //Mount System
            mform.Log("Mounting /system as RW");
            sendCommand("su -c 'mount -o rw,remount /system'");

            //keep screen on 
            sendCommand("svc power stayon usb");

            //Install SU
            //mform.Log("Installing SU Binary.");
            pushFile(defpath + "RootFiles", "su", "/data/local/tmp/");
            sendCommand("su -c 'cat /data/local/tmp/su > /system/xbin/su'");
            updateProgress();

            sendCommand("su -c 'chown 0:0 /system/xbin/su'");
            sendCommand("su -c 'chmod 6755 /system/xbin/su'");
            sendCommand("su -c 'ln -sf /system/xbin/su /system/bin/su'");

            //Run the SuperSU install ZIP
            mform.Log("Settings up SU Binary.");
           // pushFile(defpath + "RootFiles", "supersu.zip", "/data/local/tmp/");
           // sendCommand("unzip /data/local/tmp/supersu.zip META-INF/com/google/android/* -d /tmp");
           // sendCommand("sh /tmp/META-INF/com/google/android/update-binary dummy 1 /data/local/tmp/supersu.zip");
            sendCommand("su -c 'rm -Rf /data/SuperSU.apk'");

            //Make Directories for apps 
            mform.Log("Preparing device for Monitor Health Apps.");
            sendCommand("su -c 'mkdir /system/priv-app/MDM'");
            sendCommand("su -c 'mkdir /system/priv-app/MonitorHealth'");
            sendCommand("su -c 'mkdir /system/priv-app/StatusBar'");
            sendCommand("su -c 'chmod 0755 /system/priv-app/MDM'");
            sendCommand("su -c 'chmod 0755 /system/priv-app/MonitorHealth'");
            sendCommand("su -c 'chmod 0755 /system/priv-app/StatusBar'");


            //Push Monitor Health Apps 
            mform.Log("Pushing Monitor Health Apps");
            pushFile(defpath + "AppFiles", "libnetguard.so", "/data/local/tmp/");
            sendCommand("su -c 'cat /data/local/tmp/libnetguard.so > /system/lib/libnetguard.so'");
            sendCommand("su -c 'chmod 0755 /system/lib/libnetguard.so'");
            sendCommand("su -c 'chown root.root /system/lib/libnetguard.so'");
            updateProgress();
            pushFile(defpath + "AppFiles", "libopentok.so", "/data/local/tmp/");
            sendCommand("su -c 'cat /data/local/tmp/libopentok.so > /system/lib/libopentok.so'");
            sendCommand("su -c 'chmod 0755 /system/lib/libopentok.so'");
            sendCommand("su -c 'chown root.root /system/lib/libopentok.so'");
            updateProgress();
            pushFile(defpath + "AppFiles", "MDMControlPanel.apk", "/data/local/tmp/");
            sendCommand("su -c 'cat /data/local/tmp/MDMControlPanel.apk > /system/priv-app/MDM/MDMControlPanel.apk'");
            updateProgress();
            pushFile(defpath + "AppFiles", "monitorhealth1.3.6.apk", "/data/local/tmp/");
            sendCommand("su -c 'cat /data/local/tmp/monitorhealth1.3.6.apk > /system/priv-app/MonitorHealth/monitorhealth1.3.6.apk'");
            updateProgress();
            pushFile(defpath + "AppFiles", "StatusBar.apk", "/data/local/tmp/");
            sendCommand("su -c 'cat /data/local/tmp/StatusBar.apk > /system/priv-app/StatusBar/StatusBar.apk'");
            updateProgress();

            //remove Launcher Apps  
            mform.Log("Removing Unwanted Apps.");
            sendCommand("su -c 'rm -Rf /system/priv-app/TouchWizHome_2016C'");
            sendCommand("su -c 'rm -Rf /system/priv-app/EasyLauncher2_Zero'");

            //Removes Apps
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
            sendCommand("su -c 'rm -Rf /system/priv-app/SecSetupWizard2015'"); 

            sendCommand("su -c 'rm /system/etc/security/otacerts.zip'");

            //Set selinux permissions for System Apps  
            mform.Log("Restoring SELinux Permissions");
            sendCommand("su -c 'restorecon -R -v /system/priv-app'");

            //reboot to system
            mform.Log("Rebooting Device...");
            sendCommand("reboot");

            mform.Log("Waiting For Device...");
            waitForDevice();

            //Mount System R/W
            sendCommand("su -c 'mount -o rw,remount /system'");

            //settings config
            mform.Log("Configure Settings.");
            sendCommand("su -c 'settings put secure lockscreen.disabled 1'");
            sendCommand("su -c 'rm /data/system/locksettings.db'");
            sendCommand("su -c 'rm /data/system/locksettings.db-shm'");
            sendCommand("su -c 'rm /data/system/locksettings.db-wal'");
            sendCommand("su -c 'settings put secure install_non_market_apps 1'");
            sendCommand("su -c 'settings put system double_tab_launch_component a'");
            //Disable Location Services. 
            sendCommand("su -c 'settings put secure location_providers_allowed -gps'");
            sendCommand("su -c 'settings put secure location_providers_allowed -network'");
            //Disable Camera Double Home press
            sendCommand("su -c 'settings put system double_tab_launch_component \' \''");

            //Swipe on Lock Screen
            mform.Log("Opening Lock Screen");
            Thread.Sleep(500);
            sendCommand("input swipe 373 1040 373 500");

            //Touch Home 
            Thread.Sleep(500);
            sendCommand("input keyevent 3");

            mform.Log("Waiting for Permissions Dialog...");
            waitForPermissions();

            //Touch Allow Permissions
            mform.Log("Allowing Permissions");
            sendCommand("input tap 579 789");
            sendCommand("input tap 579 789");
            sendCommand("input tap 579 789");
            sendCommand("input tap 579 789");

            mform.Log("Waiting for VPN Dialog...");
            waitForVPN();

            //Touch Allow Connection
            mform.Log("Allowing VPN Access.");
            sendCommand("input tap 600 938");

            completeProgress();

            mform.Log("Device Programming Complete!");

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

            if (fmtString(firmVersion) == FIRMWARE_VERSION && fmtString(bootVersion) == BOOTLOADER_VERSION && fmtString(idVersion) == ID_VERSION
                    || fmtString(firmVersion) == FIRMWARE_VERSION_1 && fmtString(bootVersion) == BOOTLOADER_VERSION_1 && fmtString(idVersion) == ID_VERSION_1)
                return true;
            else
                return false;
        }

        private void waitForPermissions()
        {
            Console.WriteLine("Waiting For App Permissions");
            string response = sendCommandWithResponse("dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'");
            while (!response.Contains("permission.ui.GrantPermissionsActivity")) { }
            Thread.Sleep(500);
        }

        private void waitForVPN()
        {
            Console.WriteLine("Waiting For VPN Permissions");
            string response = sendCommandWithResponse("dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'");
            while (!response.Contains("com.android.vpndialogs/.ConfirmDialog")) { }
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
        private void flashRecovery()
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
                TestStack.White.UIItems.Button btnBootloader = window.Get<TestStack.White.UIItems.Button>("AP");
                btnBootloader.Click();

                //Get OpenFileDialog as child.
                List<Window> modalWindows = window.ModalWindows(); //list of all the modal windows belong to the window.
                Window childWindow = window.ModalWindow("Open"); //modal window with title "child"
                if (childWindow.IsModal)
                {
                    //Handle OpenFileDialog, Navigate to file and select it.
                    TestStack.White.UIItems.ListBoxItems.ComboBox filePaths;
                    filePaths = childWindow.Get<TestStack.White.UIItems.ListBoxItems.ComboBox>(TestStack.White.UIItems.Finders.SearchCriteria.ByAutomationId("1148"));
                    filePaths.EditableText = AppDomain.CurrentDomain.BaseDirectory + "RootFiles\\CF-Auto-Root.tar";
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
        private void pushFile(string filePath, string filename, string target)
        {
            AdbSocket adbSocket = new AdbSocket(AdbClient.Instance.EndPoint);

            waitForDevice();

            try
            {
                Console.WriteLine("Pushing File : " + filename);
                using (SyncService service = new SyncService(adbSocket, mDevice))
                using (Stream stream = File.OpenRead(filePath + "\\" + filename))
                {
                    service.Push(stream, target + filename, 0755, dt, null, CancellationToken.None);
                }
            }
            catch (Exception s) { Console.WriteLine("Error Pushing File : " + s); }
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
            updateProgress();
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
            updateProgress();
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
            Console.WriteLine("Waiting for device...");

            while (!mform.connected) { }
            Console.WriteLine("Device FOUND!");

            Thread.Sleep(500);
        }

        private void updateProgress()
        {
            mform.updateProgress();
        }

        private void completeProgress()
        {
            mform.progressComplete();
        }

    }
}
