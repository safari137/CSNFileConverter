using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace CSN_File_Converter
{
    public partial class Form1 : Form
    {
        private bool ReadFile = false;
        private const string Separator = "|";
        private bool WriteFile = false;
        private bool fail = false;
        private bool FtpSend = false;

        private LinkedList<inventory> inventoryList = new LinkedList<inventory>();


        public Form1()
        {
            InitializeComponent();
            settings_info.ReadSettings();

            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = 1;
            this.progressBar2.Minimum = 0;
            this.progressBar2.Maximum = 1;
            this.progressBar3.Minimum = 0;
            this.progressBar3.Maximum = 1;
            this.progressBar4.Minimum = 0;
            this.progressBar4.Maximum = 3;

            timer1.Interval = 2000;
        }

        private string BuildLine(inventory inv)
        {
            return (settings_info.SupplierID + Separator + inv.ItemNumber + Separator + inv.QtyOnHand + Separator + inv.QtyBackOrdered + Separator + inv.QtyOnOrder + Separator + inv.NextAvailDate + Separator + inv.Discontinued + Separator + inv.Description);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            inventoryList.Clear();
            if (fail)
            {
                ResetValues();
                fail = false;
            }
            ReadFile = true;
            timer1.Start();
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            ProcessReadFile();
            ReadFile = false;
        }

        private bool CheckCurrent(DateTime fileTime, out int days)
        {
            DateTime now = DateTime.Now;
            int num = 0;
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

        private string DataStrip(string str)
        {
            string str2 = "";
            foreach (char ch in str)
            {
                if (IsLegal(ch))
                {
                    str2 = str2 + ch.ToString();
                }
            }
            return str2;
        }

        private bool IsLegal(char c)
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
            Ftp.SendFile(settings_info.outputFile, "/inventory/" + settings_info.outputFile);
            if (Ftp.success)
            {
                fail = true;
                progressBar3.Increment(1);
                progressBar4.Increment(1);
                label5.Text = "100% Complete";
            }
            else
            {
                label4.Text = "Failed!  Fix Reported Errors and Try Again!";
                fail = true;
            }
        }

        private void ProcessReadFile()
        {
            int days = 0;
            if (((settings_info.inputFile == null) || (settings_info.outputFile == null)) || (settings_info.SupplierID == null))
            {
                MessageBox.Show("Please fill in the data under the settings window and then try again.");
                fail = true;
            }
            else if (!File.Exists(settings_info.inputFile))
            {
                MessageBox.Show("Cannot find the file: '" + settings_info.inputFile + "'.  Make sure the file is in the same directory as this program and try again.");
                fail = true;
            }
            else
            {
                DateTime lastWriteTime = File.GetLastWriteTime(settings_info.inputFile);
                if (!CheckCurrent(lastWriteTime, out days) && (MessageBox.Show("The input file is about " + days.ToString() + " day(s) old. Would you like to use it anyways?", "Old File?", MessageBoxButtons.YesNo) == DialogResult.No))
                {
                    fail = true;
                }
                else
                {
                    try
                    {
                        File.OpenRead(settings_info.inputFile);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        label4.Text = "Failed!  Fix Reported Errors and Try Again!";
                        fail = true;
                        return;
                    }
                    ReadSourceFile();
                    progressBar1.Increment(1);
                    progressBar4.Increment(1);
                    label5.Text = "33% Complete";
                    WriteFile = true;
                }
            }
        }

        private void ProcessWriteFile()
        {
            if (!WriteNewFile())
            {
                label4.Text = "Failed!  Fix Reported Errors and Try Again!";
                fail = true;
            }
            else
            {
                progressBar2.Increment(1);
                progressBar4.Increment(1);
                label5.Text = "66% Complete";
                FtpSend = true;
            }
        }

        private bool ReadSourceFile()
        {
            foreach (string str2 in File.ReadAllLines(settings_info.inputFile))
            {
                inventory inventory = new inventory();


                int index = str2.IndexOf(",");
                string str = str2.Substring(index + 1);
                
                // First Element in file (Item #)
                index = str.IndexOf(",");
                inventory.ItemNumber = str.Remove(index);
                inventory.ItemNumber = DataStrip(inventory.ItemNumber);
                inventory.ItemNumber = StripItem(inventory.ItemNumber);
                str = str.Substring(index + 1);
                
                // Second Element in file (Qty on hand)
                index = str.IndexOf(",");
                inventory.QtyOnHand = str.Remove(index);
                inventory.QtyOnHand = DataStrip(inventory.QtyOnHand);
                str = str.Substring(index + 1);
                
                // Third Element in file (Qty on order)
                index = str.IndexOf(",");
                inventory.QtyOnOrder = str.Remove(index);
                inventory.QtyOnOrder = DataStrip(inventory.QtyOnOrder);
                
                // Get Description
                inventory.Description = str.Substring(index + 1);
                inventory.Description = DataStrip(inventory.Description);
                inventoryList.AddLast(inventory);

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

        private string StripItem(string item)
        {
            int index = item.IndexOf(":");
            if (index > 0)
            {
                return item.Substring(index + 1);
            }
            return item;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (WriteFile)
            {              
                WriteFile = false;
                ProcessWriteFile();
            }
            else if (FtpSend)
            {
                FtpSend = false;
                ProcessFtpSend();
            }
            else if (!ReadFile)
            {
                timer1.Stop();
                button1.Enabled = true;
            }
        }

        private bool WriteNewFile()
        {
            string[] contents = new string[inventoryList.Count];
            int index = 0;
            foreach (inventory inventory in inventoryList)
            {
                contents[index] = BuildLine(inventory);
                index++;
            }
            try
            {
                File.WriteAllLines(settings_info.outputFile, contents);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                inventoryList.Clear();
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
