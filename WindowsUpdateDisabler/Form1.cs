using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WindowsUpdateDisabler
{
    public partial class frmMain : Form
    {
        private string LogMessage = "";
        private bool IsMute;

        public frmMain()
        {
            InitializeComponent();
            
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                lblStatus.Text = "Running";
                txtLog.Text += "--- Application Started ---" + Environment.NewLine;
                backgroundWorker1.WorkerReportsProgress = true;
                backgroundWorker1.RunWorkerAsync();
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (!worker.CancellationPending)
            {

                WindowsServiceMonitor windowsServiceMonitor = new WindowsServiceMonitor("Windows Update");

                LogMessage = "wait";
                worker.ReportProgress(25);
                windowsServiceMonitor.WaitForStart();
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                LogMessage = "start";
                worker.ReportProgress(50);

                if (windowsServiceMonitor.IsRunning)
                {
                    windowsServiceMonitor.Stop();
                    if (!IsMute)
                    {
                        Console.Beep(1000, 250);
                        Console.Beep(1000, 250);
                    }
                    LogMessage = "stop";
                    worker.ReportProgress(75);
                }

                if (!windowsServiceMonitor.IsDisabled)
                {
                    windowsServiceMonitor.Disable();
                    LogMessage = "disable";
                    worker.ReportProgress(100);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Log(LogMessage);
        }

        public void Log(string message)
        {
            string path = Environment.CurrentDirectory + "\\wudlog.txt";

            switch (message)
            {
                case "wait":
                    string logWait = Environment.NewLine + "==== Waiting for Windows Update Service to Start ==== " + Environment.NewLine;
                    txtLog.Text += logWait;
                    File.AppendAllText(path, logWait);
                    break;

                case "start":
                    string logStart = "Windows Update Service Started -- " + DateTime.Now + Environment.NewLine;
                    txtLog.Text += logStart;
                    File.AppendAllText(path, logStart);
                    break;

                case "stop":
                    string logStop = "Windows Update Service Stopped -- " + DateTime.Now + Environment.NewLine;
                    txtLog.Text += logStop;
                    File.AppendAllText(path, logStop);
                    break;

                case "disable":
                    string logDisable = "Windows Update Service Disabled -- " + DateTime.Now + Environment.NewLine;
                    txtLog.Text += logDisable;
                    File.AppendAllText(path, logDisable);
                    break;

                default:
                    break;
            }
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            

            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();
            aboutBox.Show();
        }

        private void btnMute_Click(object sender, EventArgs e)
        {
            if (!IsMute)
            {
                IsMute = true;
                txtLog.Text += "--- Notification Beeb Disabled ---" + Environment.NewLine;
            }
            else
            {
                IsMute = false;
                txtLog.Text += "--- Notification Beeb Enabled ---" + Environment.NewLine;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation)
            {
                backgroundWorker1.CancelAsync();
                backgroundWorker1.Dispose();
                lblStatus.Text = "Stopped";
                txtLog.Text += "--- Application Stopped ---" + Environment.NewLine;
            }
        }

    }
}
