using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Web;

namespace UploaderTestCSharp
{

    public class MonitorClass
    {

        const string LOG_NAME = "Log";
        private static LogString logger = LogString.GetLogString(LOG_NAME);
        
        static public Dictionary<string, string> dict;
        static string currentlyUpdating = null;

        string wowDirectory;
        string[] accountNames;
        string[] characterNames;

        public MonitorClass()
        {
            wowDirectory = Properties.Settings.Default.wowDirectory + "\\WTF\\Account";
            accountNames = Properties.Settings.Default.accountNames.Split(',');
            characterNames = Properties.Settings.Default.characterNames.Split(',');

            dict = new Dictionary<string, string>();

            foreach (string accountName in accountNames)
            {
                string luaFile = wowDirectory + "\\" + accountName.Trim().ToUpper() + "\\SavedVariables\\AuctionWhore.lua";
                FileInfo fileInfo = new FileInfo(luaFile);
                if (fileInfo.Exists) {
                    dict.Add(luaFile, fileInfo.LastWriteTime.ToLongTimeString());
                }
            }

            if (Valid())
            {
                StartWatch();
            }
        }

        public bool Valid()
        {
            return accountNames.Length > 0;
        }

        public void StartWatch()
        {
            string changedFile = null;
            string changedTime = null;

            foreach (KeyValuePair<string, string> pair in dict)
            {
                FileInfo fileInfo = new FileInfo(pair.Key);
                string newTime = fileInfo.LastWriteTime.ToLongTimeString();
                logger.Add(string.Format("Checking file: {0}", pair.Key));

                if (pair.Value != newTime)
                {
                    changedFile = pair.Key;
                    changedTime = newTime;
                    break;
                }
            }

            if (changedFile != null)
            {
                logger.Add(string.Format("Found change in: {0}", changedFile));
                StartUpdate(changedFile, changedTime);
            }
        }

        public void StartUpdate(string luaFile, string newTime)
        {
            if (currentlyUpdating == null)
            {
                currentlyUpdating = luaFile;   
                WebClient client = new WebClient();
                client.UploadFileCompleted += On_File_Upload;

                string apiKey = Properties.Settings.Default.apiKey;

                if(apiKey == ""){
                    logger.Add(string.Format("Missing API Key"));
                    currentlyUpdating = null;
                } else {
                    logger.Add(string.Format("Starting upload of: {0}", currentlyUpdating));
                    Uri uri = new Uri("http://auctionwhore.keithpitt.com/api/upload_scan?api_key=" + System.Uri.EscapeDataString(apiKey));
                    client.UploadFileAsync(uri, luaFile);
                    dict[currentlyUpdating] = newTime;
                }
            }
        }

        static void On_File_Upload(object sender, EventArgs e)
        {
            if (File.Exists(currentlyUpdating + ".ac.bak"))
            {
                logger.Add(string.Format("Creating backup"));
                File.Delete(currentlyUpdating + ".ac.bak");
            }
            File.Move(currentlyUpdating, currentlyUpdating + ".ac.bak");

            logger.Add(string.Format("Creating new {0}", currentlyUpdating));
            FileStream file = new FileStream(currentlyUpdating, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.Write("");
            sw.Close();
            file.Close();

            FileInfo fileInfo = new FileInfo(currentlyUpdating);
            dict[currentlyUpdating] = fileInfo.LastWriteTime.ToLongTimeString();
            currentlyUpdating = null;
        }

    }

    class Main
    {
        static NotifyIcon notificationIcon;
        static ContextMenu contextMenu;
        static Timer timer;
        static MonitorClass monitor;

        public Main()
        {
            // Setup the context menu
            contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(0, new MenuItem("About", new System.EventHandler(On_About_Click)));
            contextMenu.MenuItems.Add(1, new MenuItem("Logs", new System.EventHandler(On_Log_Click)));
            contextMenu.MenuItems.Add(2, new MenuItem("Settings", new System.EventHandler(On_Settings_Click)));
            contextMenu.MenuItems.Add(3, new MenuItem("Exit", new System.EventHandler(On_Exit_Click)));

            // Setup the notification icon.
            notificationIcon = new NotifyIcon();
            notificationIcon.Visible = true;
            notificationIcon.Icon = global::UploaderTestCSharp.Properties.Resources.gremlin_16;
            notificationIcon.ContextMenu = contextMenu;

            // Start the thread that wactches the AuctionWhore lua file.
            monitor = new MonitorClass();

            timer = new Timer();
            timer.Interval = 5000;
            timer.Start();
            timer.Tick += new EventHandler(Timer_Tick);
        }

        static void Timer_Tick(object sender, EventArgs e)
        {
            monitor.StartWatch();
        }

        static void On_Log_Click(object sender, EventArgs e)
        {
            LoggerBox loggerBox = new LoggerBox();
            loggerBox.Show();
        }

        static void On_About_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }

        static void On_Settings_Click(object sender, EventArgs e)
        {
            Settings settingsBox = new Settings();
            settingsBox.ShowDialog();
        }

        static void On_Exit_Click(object sender, EventArgs e)
        {
            notificationIcon.Visible = false;
            Application.Exit();
        }

    }
}
