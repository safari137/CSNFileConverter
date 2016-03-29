using System;
using System.IO;
using System.Windows.Forms;

namespace CSN_File_Converter.Service
{
    public static class SettingsService
    {
        private const string SettingsFile = "settings.data";

        public static string FtpHost = null;
        public static string FtpPass = null;
        public static string FtpUser = null;
        public static string InputFile = null;
        public static string OutputFile = null;
        public static string SupplierId = null;

        public static void DefaultSettings()
        {
        }

        public static void ReadSettings()
        {
            if (!File.Exists(SettingsFile))
            {
                DefaultSettings();
                MessageBox.Show("Not Found!");
                return;
            }

            foreach (var line in File.ReadAllLines(SettingsFile))
            {
                var startIndex = line.IndexOf(":");
                var itemTitle = line.Remove(startIndex);
                var itemValue = line.Substring(startIndex + 1);

                switch (itemTitle)
                {
                    case "SUPPLIERID":
                        SupplierId = itemValue;
                        break;

                    case "FTPHOST":
                        FtpHost = itemValue;
                        break;

                    case "FTPUSER":
                        FtpUser = itemValue;
                        break;

                    case "FTPPASS":
                        FtpPass = itemValue;
                        break;

                    case "INPUTFILE":
                        InputFile = itemValue;
                        break;

                    case "OUTPUTFILE":
                        OutputFile = itemValue;
                        break;
                }
            }
        }

        public static void SaveSettings()
        {
            var contents = new string[] { "SUPPLIERID:" + SupplierId, "FTPHOST:" + FtpHost, "FTPUSER:" + FtpUser, "FTPPASS:" + FtpPass, "INPUTFILE:" + InputFile, "OUTPUTFILE:" + OutputFile };
            try
            {
                File.WriteAllLines(SettingsFile, contents);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
