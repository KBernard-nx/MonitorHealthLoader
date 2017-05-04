using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpAdbClient;
using System.IO;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace MonitorHealthLoader
{
    public partial class Form1 : Form
    {

        public SharpAdbClient.AdbServer sadb;
        AdbSocket adbSocket;
        AdbClient adbClient;

        public Form1()
        {
            InitializeComponent();

            //Kill Any ADB Servers Running.
            foreach (var process in Process.GetProcessesByName("adb.exe"))
            {
                process.Kill();
            }

            //Start ADB Server 
            sadb = new AdbServer();
            sadb.StartServer(AppDomain.CurrentDomain.BaseDirectory + "ADB/adb.exe", restartServerIfNewer: false);

            //Create ADB Client
            adbClient = new AdbClient();

            //Setup ADB Socket
            adbSocket = new AdbSocket(adbClient.EndPoint);

            //Show Form.
            this.Show();

            //Start Monitor
            StartMonitor();
            
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            
            var devices = AdbClient.Instance.GetDevices();

            foreach (var device in devices)
            {
                Log(device.Name.ToString());
            }
            
            J320A j = new J320A((DeviceData)devices[0], adbSocket, this);

        }

        void StartMonitor()
        {
             Log("Starting Monitor");
             var monitor = new DeviceMonitor(adbSocket);
             monitor.DeviceConnected += this.OnDeviceConnected;
             monitor.DeviceDisconnected += this.OnDeviceDisconnected;
             monitor.DeviceChanged += this.OnDeviceChanged;
             monitor.Start();
        }

        void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {

        }

        void OnDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            var devices = AdbClient.Instance.GetDevices();
            Log("==========================================");
            Log(devices[0].Name.ToString() + " has connected to this PC");
            Log("Name: " + devices[0].Name.ToString());
            Log("Model: " + devices[0].Model.ToString());
            Log("Product: " + devices[0].Product.ToString());
            Log("Serial: " + devices[0].Serial.ToString());
            Log("State: " + devices[0].State.ToString());
            Log("==========================================");

        }

        void OnDeviceDisconnected(object sender, DeviceDataEventArgs e)
        {
            Log("");
            Log("----------------------------------------------------");
            Log("Device has Disconnected from this PC");
            Log("----------------------------------------------------");
            Log("");
        }

        void Push(String filename)
        {
            Log("Pushing File");
            DateTime dt = new DateTime();
           
            var device = AdbClient.Instance.GetDevices().First();

            using (SyncService service = new SyncService(adbSocket, device))
            using (Stream stream = File.OpenRead(@"G:\j320\Process_Monitor_Health\files\Kingroot.apk"))
            {
                service.Push(stream, "/data/local/tmp/Kingroot.apk" + filename, 0444, dt, null, CancellationToken.None);
            }
        }

        void EchoTest()
        {
            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();

            AdbClient.Instance.ExecuteRemoteCommand("ls /data/local/tmp", device, receiver);

            Console.WriteLine("The device responded:");
            Console.WriteLine(receiver.ToString());
            Log("The device responded:");
            Log(receiver.ToString());
        }


        public void Log(String log)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(Log), new object[] { log });
                return;
            }

            textBox1.AppendText(Environment.NewLine + log); 

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            adbClient.KillAdb();
        }
    }
}
