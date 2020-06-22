using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Net;
using System.Media;
using System.Diagnostics;

namespace connectionChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        String soundDir = "sounds";
        String alaram = @"sounds\ta_da.wav";

        String reportsDir = "reports";
        String activeReport = null;

        String activeURI = null;

        StreamWriter reportWrite;

        public delegate void connectionOkDel();
        public delegate void connectionErrDel();
        public delegate void enButton();

        connectionOkDel conOk;
        connectionErrDel conErr;
        enButton enBtn;

        SoundPlayer player = null;

        bool isStopped = false;

        public void ConnectionOK()
        {
            this.statusText.Invoke((MethodInvoker)delegate
            {
                this.statusText.Text = "CONNECTION NORMAL";
            });
        }

        public void ConnectionERR()
        {
            if (player != null)
            {
                player.Play();
            }
            this.statusText.Invoke((MethodInvoker)delegate
            {
                this.statusText.Text = "CONNECTION ERROR";
            });
        }

        public void enableStartButton()
        {
            this.buttonStart.Invoke((MethodInvoker)delegate
            {
                this.buttonStart.Enabled = true;
            });
            this.statusText.Invoke((MethodInvoker)delegate
            {
                this.statusText.Text = "STOPPED";
            });
        }

        public void checkConnection()
        {
            while (true)
            {
                if (isStopped)
                { 
                    enBtn.Invoke();
                    isStopped = false;
                    return;
                }

                Thread.Sleep(500);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(activeURI);
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    Stream temp = response.GetResponseStream();
                    StreamReader sr = new StreamReader(temp);
                    sr.ReadToEnd();
                    //connection OK
                    conOk.Invoke();
                }
                catch (WebException ex)
                {
                    //Conncetion ERR
                    String msg = ex.Message;
                    conErr.Invoke();
                    reportWrite.WriteLine("ОТКАЗ::" + DateTime.Now.ToString());
                }
            }
        }

        Thread check = null;

        private void buttonStart_Click(object sender, EventArgs e)
        {
            activeURI = URIText.Text;
            String date = DateTime.Now.ToString();

            date = date.Replace(":", "_");

            activeReport = date + "_report.txt";

            reportWrite = new StreamWriter(reportsDir+@"\"+ activeReport);

            check = new Thread(checkConnection);

            check.Start();

            buttonStart.Enabled = false;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            isStopped = true;
            if (reportWrite != null)
                reportWrite.Close();
            if (isStopped == false)
            {
                check.Abort();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            conOk = ConnectionOK;
            conErr = ConnectionERR;
            enBtn = enableStartButton;

            if (Directory.Exists(soundDir))
            {
                player = new SoundPlayer(alaram);
            }
            else
            {
                MessageBox.Show("Папка со звуками не найдена!", "АЛЯРМА!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Directory.CreateDirectory(soundDir);
            }

            if (!Directory.Exists(reportsDir))
            {
                Directory.CreateDirectory(reportsDir);
            }

            check = new Thread(checkConnection);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(reportsDir);
        }
    }
}