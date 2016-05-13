using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;

namespace Sleep_In
{
    public partial class SleepForm : Form
    {
           

        Thread timerThread;

        
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int ExitWindowsEx(int flag, int reserved);

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        static volatile bool keepRunning = false;
        public SleepForm()
        {
            InitializeComponent();
            minutes.Value = 10;
            checkBox2.Checked = true;
        }

        private void Shutdown()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    Shutdown();
                }));
            }
            else
            {
                var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
        }
        private void Mute()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    Mute();
                }));
            }
            else
            {
                SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                    (IntPtr)APPCOMMAND_VOLUME_MUTE);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(timerThread ==null || !timerThread.IsAlive)
            {
                keepRunning = true;
                timerThread = new Thread(new ThreadStart(timerThreadFunction));
                timerThread.Start();
            }
            else if(timerThread.IsAlive)
            {
                keepRunning = false;
                timerThread.Abort();
                enableControls();
            }
        }

        private void disableControls()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    disableControls();
                }));
            }
            else
            {
                button1.Text = "Stop";
                hours.Enabled = false;
                minutes.Enabled = false;
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
            }
        }

        private void enableControls()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    enableControls();
                }));
            }
            else
            {
                button1.Text = "Start Timer";
                hours.Enabled = true;
                minutes.Enabled = true;
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
            }
        }

        private void showTimer(int totalTime)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    showTimer(totalTime);
                }));
            }
            else
            {
                int hours_value = totalTime / 3600;
                totalTime -= hours_value * 3600;
                int minutes_value = totalTime / 60;

                hours.Value = hours_value;
                minutes.Value = minutes_value;
            }
        }

        private void timerThreadFunction()
        {
            disableControls();

            int hours_to_sec = (int)hours.Value * 3600;
            int minutes_to_sec = (int)minutes.Value * 60;

            int totalTime = hours_to_sec + minutes_to_sec;

            while(keepRunning)
            {
                if (totalTime > 0)
                    totalTime--;
                else
                {
                    keepRunning = false;
                    if(checkBox1.Checked)
                    {
                        this.Shutdown();
                    }
                    if(checkBox2.Checked && !checkBox1.Checked)
                    {
                        this.Mute();
                    }
                }
                showTimer(totalTime);
                Thread.Sleep(1000);
            }
                enableControls();
        }
    }
}
