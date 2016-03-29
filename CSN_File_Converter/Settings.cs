using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSN_File_Converter
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            this.textBox6.Text = settings_info.ftpPass;
            this.textBox5.Text = settings_info.ftpUser;
            this.textBox4.Text = settings_info.ftpHost;
            this.textBox3.Text = settings_info.outputFile;
            this.textBox2.Text = settings_info.SupplierID;
            this.textBox1.Text = settings_info.inputFile;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = dialog.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            settings_info.inputFile = this.textBox1.Text;
            settings_info.ftpPass = this.textBox6.Text;
            settings_info.ftpUser = this.textBox5.Text;
            settings_info.ftpHost = this.textBox4.Text;
            settings_info.outputFile = this.textBox3.Text;
            settings_info.SupplierID = this.textBox2.Text;
            settings_info.SaveSettings();
            base.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
