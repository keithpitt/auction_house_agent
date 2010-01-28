using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace UploaderTestCSharp
{
    public partial class LoggerBox : Form
    {

        const string LOG_NAME = "Log";
        private LogString myLogger = LogString.GetLogString(LOG_NAME);

        public LoggerBox()
        {
            // Logging option changes:
            //myLogger.ReverseOrder = false;      // Append new items
            //myLogger.Timestamp = false;         // No timestamp
            //myLogger.MaxChars = 500;            // Test log truncation
            // ...
            InitializeComponent();
            txtLog.ScrollBars = ScrollBars.Both; // use scroll bars; no text wrapping
            txtLog.MaxLength = myLogger.MaxChars + 100;
            // Add update callback delegate
            myLogger.OnLogUpdate += new LogString.LogUpdateDelegate(this.LogUpdate);
        }
        // Updates that come from a different thread can not directly change the
        // TextBox component. This must be done through Invoke().
        private delegate void UpdateDelegate();
        private void LogUpdate()
        {
            Invoke(new UpdateDelegate(
                delegate
                {
                    txtLog.Text = myLogger.Log;
                })
            );
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            txtLog.Text = myLogger.Log;
        }
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            myLogger.Clear();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Remove my update delegate
            myLogger.OnLogUpdate -= new LogString.LogUpdateDelegate(this.LogUpdate);
            // Save all logs.
            LogString.PersistAll();
        }

    }
}