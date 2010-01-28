using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UploaderTestCSharp
{
    public partial class Settings : Form
    {

        public Settings()
        {
            InitializeComponent();
            directoryTextBox.Text = Properties.Settings.Default.wowDirectory;
            accountsTextBox.Text = Properties.Settings.Default.accountNames;
            charactersTextBox.Text = Properties.Settings.Default.characterNames;
            apiTextBox.Text = Properties.Settings.Default.apiKey;
        }

        public void Save() {
            Properties.Settings.Default.wowDirectory = directoryTextBox.Text;
            Properties.Settings.Default.accountNames = accountsTextBox.Text;
            Properties.Settings.Default.characterNames = charactersTextBox.Text;
            Properties.Settings.Default.apiKey = apiTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Save();
            Hide();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            DialogResult result = folderBrowser.ShowDialog();

            if (result == DialogResult.OK)
            {
                directoryTextBox.Text = folderBrowser.SelectedPath.ToString();
            }
        }

    }
}
