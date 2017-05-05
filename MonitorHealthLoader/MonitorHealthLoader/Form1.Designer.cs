namespace MonitorHealthLoader
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listDevices = new System.Windows.Forms.ListView();
            this.colNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDeviceName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSerial = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProgress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(372, 415);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(211, 58);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(118, 167);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(700, 242);
            this.textBox1.TabIndex = 1;
            // 
            // listDevices
            // 
            this.listDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colNumber,
            this.colDeviceName,
            this.colSerial,
            this.colStatus,
            this.colVersion,
            this.colProgress});
            this.listDevices.Location = new System.Drawing.Point(118, 41);
            this.listDevices.Name = "listDevices";
            this.listDevices.Size = new System.Drawing.Size(700, 103);
            this.listDevices.TabIndex = 2;
            this.listDevices.UseCompatibleStateImageBehavior = false;
            this.listDevices.View = System.Windows.Forms.View.Details;
            // 
            // colNumber
            // 
            this.colNumber.Text = "#";
            // 
            // colDeviceName
            // 
            this.colDeviceName.Text = "Name";
            this.colDeviceName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colDeviceName.Width = 121;
            // 
            // colSerial
            // 
            this.colSerial.Text = "Serial";
            this.colSerial.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colSerial.Width = 98;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colStatus.Width = 100;
            // 
            // colVersion
            // 
            this.colVersion.Text = "Firmware Ver.";
            this.colVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colVersion.Width = 178;
            // 
            // colProgress
            // 
            this.colProgress.Text = "Progress";
            this.colProgress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colProgress.Width = 138;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(941, 557);
            this.Controls.Add(this.listDevices);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView listDevices;
        private System.Windows.Forms.ColumnHeader colNumber;
        private System.Windows.Forms.ColumnHeader colDeviceName;
        private System.Windows.Forms.ColumnHeader colSerial;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colVersion;
        private System.Windows.Forms.ColumnHeader colProgress;
    }
}

