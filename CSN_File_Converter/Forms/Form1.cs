using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CSN_File_Converter.Model;
using CSN_File_Converter.Service;

namespace CSN_File_Converter.Forms
{
    public partial class Form1 : Form
    {
        private const string Separator = "|";
        private bool _inputFileHasBeenRead = false;
        private bool _shouldWriteFile = false;
        private bool _conversionFailed = false;
        private bool _shouldFtpSend = false;

        private readonly LinkedList<Inventory> _inventoryList = new LinkedList<Inventory>();


        public Form1()
        {
            InitializeComponent();
            SettingsService.ReadSettings();

            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = 1;
            this.progressBar2.Minimum = 0;
            this.progressBar2.Maximum = 1;
            this.progressBar3.Minimum = 0;
            this.progressBar3.Maximum = 1;
            this.progressBar4.Minimum = 0;
            this.progressBar4.Maximum = 3;

            taskManager.Interval = 2000;
        }

        private static string BuildLine(Inventory inv)
        {
            return (SettingsService.SupplierId + Separator + inv.ItemNumber + Separator + inv.QtyOnHand + Separator + inv.QtyBackOrdered + Separator + inv.QtyOnOrder + Separator + inv.NextAvailDate + Separator + inv.Discontinued + Separator + inv.Description);
        }

        private void convertButton_Click(object sender, EventArgs e)
        {
            convertButton.Enabled = false;
            _inventoryList.Clear();
            if (_conversionFailed)
            {
                ResetValues();
                _conversionFailed = false;
            }
            _inputFileHasBeenRead = true;
            taskManager.Start();
            this.taskManager.Tick += new System.EventHandler(this.TaskManager);
            ProcessReadFile();
            _inputFileHasBeenRead = false;
        }

        private void TaskManager(object sender, EventArgs e)
        {
            if (_shouldWriteFile)
            {
                _shouldWriteFile = false;
                ProcessWriteFile();
            }
            else if (_shouldFtpSend)
            {
                _shouldFtpSend = false;
                ProcessFtpSend();
            }
            else if (!_inputFileHasBeenRead)
            {
                taskManager.Stop();
                convertButton.Enabled = true;
            }
        }

        private static bool CheckCurrent(DateTime fileTime, out int days)
        {
            var now = DateTime.Now;
            var num = 0;
            if (((fileTime.Year == now.Year) && (fileTime.Month == now.Month)) && (fileTime.Day == now.Day))
            {
                days = 0;
                return true;
            }
            num = (now.Year - fileTime.Year) * 0x16d;
            num += (now.Month - fileTime.Month) * 30;
            days = num;
            days += now.Day - fileTime.Day;
            return false;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private string DataStrip(string data)
        {
            var result = "";
            foreach (var character in data)
            {
                if (IsLegal(character))
                {
                    result = result + character.ToString();
                }
            }
            return result;
        }

        private static bool IsLegal(char c)
        {
            switch (c)
            {
                case '*':
                    return false;
                case ',':
                    return false;
                case '|':
                    return false;
                case '"':
                    return false;
            }
            return true;
        }

        private void ProcessFtpSend()
        {
            var ftp = new FtpService();

            ftp.SendFile(SettingsService.OutputFile, "/inventory/" + SettingsService.OutputFile);
            if (ftp.Success)
            {
                _conversionFailed = true;
                progressBar3.Increment(1);
                progressBar4.Increment(1);
                label5.Text = "100% Complete";
            }
            else
            {
                label4.Text = "Failed!  Fix Reported Errors and Try Again!";
                _conversionFailed = true;
            }
        }

        private void ProcessReadFile()
        {
            if (((SettingsService.InputFile == null) || (SettingsService.OutputFile == null)) ||
                (SettingsService.SupplierId == null))
            {
                MessageBox.Show("Please fill in the data under the settings window and then try again.");
                _conversionFailed = true;
                return;
            }
            if (!File.Exists(SettingsService.InputFile))
            {
                MessageBox.Show("Cannot find the file: '" + SettingsService.InputFile +
                                "'.  Make sure the file is in the same directory as this program and try again.");
                _conversionFailed = true;
                return;
            }

            var lastWriteTime = File.GetLastWriteTime(SettingsService.InputFile);
            var days = 0;
            if (!CheckCurrent(lastWriteTime, out days) &&
                (MessageBox.Show(
                    "The input file is about " + days.ToString() + " day(s) old. Would you like to use it anyways?",
                    "Old File?", MessageBoxButtons.YesNo) == DialogResult.No))
            {
                _conversionFailed = true;
                return;
            }

            try
            {
                File.OpenRead(SettingsService.InputFile);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                label4.Text = "Failed!  Fix Reported Errors and Try Again!";
                _conversionFailed = true;
                return;
            }

            ReadSourceFile();
            progressBar1.Increment(1);
            progressBar4.Increment(1);
            label5.Text = "33% Complete";
            _shouldWriteFile = true;
        }


        private void ProcessWriteFile()
        {
            if (!WriteNewFile())
            {
                label4.Text = "Failed!  Fix Reported Errors and Try Again!";
                _conversionFailed = true;
                return;
            }

            progressBar2.Increment(1);
            progressBar4.Increment(1);
            label5.Text = "66% Complete";
            _shouldFtpSend = true;
        }

        private bool ReadSourceFile()
        {
            foreach (var line in File.ReadAllLines(SettingsService.InputFile))
            {
                var inventory = new Inventory();

                if (line == null)
                    continue;

                var index = line.IndexOf(",");
                var item = line.Substring(index + 1);
                
                // First Element in file (Item #)
                index = item.IndexOf(",");
                inventory.ItemNumber = item.Remove(index);
                inventory.ItemNumber = DataStrip(inventory.ItemNumber);
                inventory.ItemNumber = StripItem(inventory.ItemNumber);
                item = item.Substring(index + 1);
                
                // Second Element in file (Qty on hand)
                index = item.IndexOf(",");
                inventory.QtyOnHand = item.Remove(index);
                inventory.QtyOnHand = DataStrip(inventory.QtyOnHand);
                item = item.Substring(index + 1);
                
                // Third Element in file (Qty on order)
                index = item.IndexOf(",");
                inventory.QtyOnOrder = item.Remove(index);
                inventory.QtyOnOrder = DataStrip(inventory.QtyOnOrder);
                
                // Get Description
                inventory.Description = item.Substring(index + 1);
                inventory.Description = DataStrip(inventory.Description);
                _inventoryList.AddLast(inventory);

                // The following items are now blank by default                
                inventory.QtyBackOrdered = "";
                inventory.NextAvailDate = "";
                inventory.Discontinued = "";
            }
            return true;
        }

        private void ResetValues()
        {
            progressBar1.Value = progressBar1.Minimum;
            progressBar2.Value = progressBar2.Minimum;
            progressBar3.Value = progressBar3.Minimum;
            progressBar4.Value = progressBar4.Minimum;
            label4.Text = ":";
            label5.Text = "0%";
        }

        private static string StripItem(string item)
        {
            var index = item.IndexOf(":");

            return index > 0 ? item.Substring(index + 1) : item;
        }

        private bool WriteNewFile()
        {
            var contents = new string[_inventoryList.Count];
            var index = 0;
            foreach (var inventoryItem in _inventoryList)
            {
                contents[index] = BuildLine(inventoryItem);
                index++;
            }
            try
            {
                File.WriteAllLines(SettingsService.OutputFile, contents);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                _inventoryList.Clear();
                return false;
            }
            return true;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Settings().ShowDialog();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void closeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
