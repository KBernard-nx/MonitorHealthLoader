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
using System.Management;
using System.Diagnostics;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.Factory;

namespace MonitorHealthLoader
{
    public partial class Form1 : Form
    {

        public SharpAdbClient.AdbServer sadb;
        AdbSocket adbSocket;
        AdbClient adbClient;
        DeviceMonitor monitor;

        public bool connected = false;

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
            sadb.StartServer(AppDomain.CurrentDomain.BaseDirectory + "ADB/adb.exe", restartServerIfNewer: true);

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
            //Start needs to work with all connected devices

            var devices = AdbClient.Instance.GetDevices();

            foreach (var device in devices)
            {
                
                if (device.Name.ToString() == "j3xlteatt")
                {
                    J320A j = new J320A((DeviceData)devices[0], adbSocket, adbClient, this);
                    Thread newThread = new Thread(new ThreadStart(j.startProcess));
                    newThread.Start();
                    
                }
                else if (device.Name.ToString() == "j5lte")
                {
                    J500M j = new J500M((DeviceData)devices[0], adbSocket, this);
                }
                else
                {
                    Log(device.Name.ToString() + " is not Configured for this process yet!");
                }
                
                
            }
            
            

        }

        void StopMonitor()
        {
            if (monitor.IsRunning)
            {
                monitor.Dispose();
            }
        }

        void StartMonitor()
        {
             Log("Starting Monitor");
             monitor = new DeviceMonitor(adbSocket);
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
            connected = true;
            //This Helps the monitor gather the device info.
            Thread.Sleep(250);

            DeviceData selectedDevice = null;
            var devices = AdbClient.Instance.GetDevices();
            foreach (var device in devices)
            {

                if (e.Device.Serial == device.Serial)
                    selectedDevice = device;       
            }

            AddDeviceList(selectedDevice);

            Log("==========================================");
            Log(selectedDevice.Name.ToString() + " has connected to this PC");
            Log("Name: " + selectedDevice.Name.ToString());
            Log("Model: " + selectedDevice.Model.ToString());
            Log("Product: " + selectedDevice.Product.ToString());
            Log("Serial: " + selectedDevice.Serial.ToString());
            Log("State: " + selectedDevice.State.ToString());
            Log("==========================================");

        }
        
        void OnDeviceDisconnected(object sender, DeviceDataEventArgs e)
        {

            connected = false;
            Log("");
            Log("----------------------------------------------------");
            Log( e.Device.Serial + " has Disconnected from this PC");
            Log("----------------------------------------------------");
            Log("");
        }

        public void AddDeviceList(DeviceData device)
        {

            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<DeviceData>(AddDeviceList), new object[] { device });
                return;
            }

            ImageList imageList = new ImageList();

            Bitmap progressBarBitmap = new Bitmap(
                imageList.ImageSize.Width,
                imageList.ImageSize.Height);
            imageList.Images.Add(progressBarBitmap);
            ProgressBar progressBar = new ProgressBar();
            progressBar.MinimumSize = imageList.ImageSize;
            progressBar.MaximumSize = imageList.ImageSize;
            progressBar.Size = imageList.ImageSize;

            // probably create also some BackgroundWorker here with information about
            // this particular progressBar

            int count = this.listDevices.Items.Count + 1;

            ListViewItem lvi = new ListViewItem(
                new[] { count.ToString(),
                        device.Name.ToString(),
                        device.Serial.ToString(),
                        device.State.ToString(),
                        device.Product.ToString()},
                this.listDevices.Items.Count);

            lvi.UseItemStyleForSubItems = true;
            this.listDevices.Items.Add(lvi);


        }

        public void Progress()
        {
      //      int previousProgress = progressBar.Value;
      //      progressBar.Value = ...
      //
      //      if (progressBar.Value != previousProgress)
      //      {
      //          progressBar.DrawToBitmap(progressBarBitmap, bounds);
      //          progressBarImageList.Images[index] = progressBarBitmap;
      //      }
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
            StopMonitor();
            adbClient.KillAdb();
        }

        private void button1_Click(object sender, EventArgs e)
        {


        }

        private void button2_Click(object sender, EventArgs e)
        {



        }
    }
}
