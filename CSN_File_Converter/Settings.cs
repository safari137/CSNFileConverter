using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSN_File_Converter.Service;

namespace CSN_File_Converter
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            this.textBox6.Text = SettingsService.FtpPass;
            this.textBox5.Text = SettingsService.FtpUser;
            this.textBox4.Text = SettingsService.FtpHost;
            this.textBox3.Text = SettingsService.OutputFile;
            this.textBox2.Text = SettingsService.SupplierId;
            this.textBox1.Text = SettingsService.InputFile;
        }

        private void chooseFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = dialog.FileName;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SettingsService.InputFile = this.textBox1.Text;
            SettingsService.FtpPass = this.textBox6.Text;
            SettingsService.FtpUser = this.textBox5.Text;
            SettingsService.FtpHost = this.textBox4.Text;
            SettingsService.OutputFile = this.textBox3.Text;
            SettingsService.SupplierId = this.textBox2.Text;
            SettingsService.SaveSettings();
            base.Close();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
