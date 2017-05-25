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
        Form authorizeDialog;
        int newProgressValue = 0;
        J320A j;
        Thread programThread;
        bool deviceProgramming = false;

        public bool connected = false;

        public Form1()
        {
            InitializeComponent();

            lblVersion.Text = Application.ProductVersion.ToString();

            //make dialog for authorize adb connection
            authorizeDialog = new Form();
            authorizeDialog.Height = 550;
            authorizeDialog.Width = 300;
            PictureBox pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "Images\\authorize.png");
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            authorizeDialog.Controls.Add(pictureBox);

            //Set Nodevice Image
            this.pictureBox1.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "Images\\nodevice.png");

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
                
                if (device.Name.ToString() == "j3xlteatt" || device.Name.ToString() == "j3xlteuc")
                {
                    deviceProgramming = true;
                    j = new J320A((DeviceData)devices[0], adbSocket, adbClient, this, this.progressBar1);
                    programThread = new Thread(new ThreadStart(j.startProcess));
                    programThread.Start();
                    
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
             Log("Device Monitor Started.");
             Log("Make sure that USB Debugging is Enabled!");
             monitor = new DeviceMonitor(adbSocket);
             monitor.DeviceConnected += this.OnDeviceConnected;
             monitor.DeviceDisconnected += this.OnDeviceDisconnected;
             monitor.DeviceChanged += this.OnDeviceChanged;
             monitor.Start();
        }

        void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {
            connected = true;
            if (deviceProgramming) { return; }

            if (e.Device.State == DeviceState.Online)
            {
                //This Helps the monitor gather the device info.
                Thread.Sleep(250);

                DeviceData selectedDevice = null;
                var devices = AdbClient.Instance.GetDevices();
                foreach (var device in devices)
                {

                    if (e.Device.Serial == device.Serial)
                        selectedDevice = device;
                }
                RemoveDeviceList(e.Device);
                AddDeviceList(selectedDevice);

                this.pictureBox1.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "Images\\j320a.png");

                Log("==========================================");
                Log(selectedDevice.Name.ToString() + " has connected to this PC");
                Log("Name: " + selectedDevice.Name.ToString());
                Log("Model: " + selectedDevice.Model.ToString());
                Log("Product: " + selectedDevice.Product.ToString());
                Log("Serial: " + selectedDevice.Serial.ToString());
                Log("State: " + selectedDevice.State.ToString());
                Log("==========================================");

            }
            if (e.Device.State == DeviceState.Unauthorized || e.Device.State == DeviceState.Offline)
            {
                Log("Authorize ADB Connection!");

                this.pictureBox1.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "Images\\authorize.png");
            }


        }

        void OnDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            connected = true;
            if (deviceProgramming) { return; }
            //This Helps the monitor gather the device info.
            Thread.Sleep(250);

            if (e.Device.State == DeviceState.Online)
            {
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

                this.pictureBox1.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "Images\\j320a.png");

                Log("==========================================");
                Log(selectedDevice.Name.ToString() + " has connected to this PC");
                Log("Name: " + selectedDevice.Name.ToString());
                Log("Model: " + selectedDevice.Model.ToString());
                Log("Product: " + selectedDevice.Product.ToString());
                Log("Serial: " + selectedDevice.Serial.ToString());
                Log("State: " + selectedDevice.State.ToString());
                Log("==========================================");

            }
            if (e.Device.State == DeviceState.Unauthorized || e.Device.State == DeviceState.Offline)
            {
                Log("Authorize ADB Connection!");
                this.pictureBox1.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "Images\\authorize.png");
            }


        }
        
        void OnDeviceDisconnected(object sender, DeviceDataEventArgs e)
        {
            connected = false;

            if (deviceProgramming) { return; }

            RemoveDeviceList(e.Device);

            //Set Nodevice Image
            this.pictureBox1.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "Images\\nodevice.png");

            Log("");
            Log("----------------------------------------------------");
            Log( e.Device.Serial + " has Disconnected from this PC");
            Log("----------------------------------------------------");
            Log("");
        }

        public void RemoveDeviceList(DeviceData device)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<DeviceData>(RemoveDeviceList), new object[] { device });
                return;
            }

            this.btnStart.Enabled = false;

            foreach (ListViewItem lvis in listDevices.Items)
            {
                foreach (var s in lvis.SubItems) {

                    if (s.ToString().Contains(device.Serial))
                    {
                        listDevices.Items.RemoveAt(lvis.Index);
                    }
                }
            }
        }

        public void AddDeviceList(DeviceData device)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<DeviceData>(AddDeviceList), new object[] { device });
                return;
            }

            this.btnStart.Enabled = true;

            foreach (ListViewItem lvis in listDevices.Items)
            {
                if (lvis.Text.Contains(device.Serial))
                {
                    listDevices.Items.Remove(lvis);
                }
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

        public void updateProgress()
        {
            if (newProgressValue >= 100)
            {
                newProgressValue = 0;
            }
            newProgressValue = newProgressValue + 1;

            MethodInvoker mi = new MethodInvoker(() => this.progressBar1.Value = newProgressValue);
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar1.Invoke(mi);
            }
            else
            {
                mi.Invoke();
            }
        }

        public void progressComplete()
        {
            deviceProgramming = false;

            MethodInvoker mi = new MethodInvoker(() => this.progressBar1.Value = 0);
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar1.Invoke(mi);
            }
            else
            {
                mi.Invoke();
            }

            MessageBox.Show("Programming Complete!","Device Status");
        }

        

        public void Log(String log)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(Log), new object[] { log });
                return;
            }

            textBox1.AppendText(Environment.NewLine + log + Environment.NewLine); 

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopMonitor();
            adbClient.KillAdb();
            Environment.Exit(0);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            adbClient.KillAdb();
            this.Close();
            Environment.Exit(0);
        }
    }
}
