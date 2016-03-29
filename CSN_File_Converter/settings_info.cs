using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace CSN_File_Converter
{
    public static class settings_info
    {
        public static string ftpHost = null;
        public static string ftpPass = null;
        public static string ftpUser = null;
        public static string inputFile = null;
        public static string outputFile = null;
        private static string settingsFile = "settings.data";
        public static string SupplierID = null;

        public static void DefaultSettings()
        {
        }

        public static void ReadSettings()
        {
            int startIndex = 0;
            if (!File.Exists(settingsFile))
            {
                DefaultSettings();
                MessageBox.Show("Not Found!");
            }
            else
            {
                foreach (string str3 in File.ReadAllLines(settingsFile))
                {
                    startIndex = str3.IndexOf(":");
                    string str = str3.Remove(startIndex);
                    string str2 = str3.Substring(startIndex + 1);
                    switch (str)
                    {
                        case "SUPPLIERID":
                            SupplierID = str2;
                            break;

                        case "FTPHOST":
                            ftpHost = str2;
                            break;

                        case "FTPUSER":
                            ftpUser = str2;
                            break;

                        case "FTPPASS":
                            ftpPass = str2;
                            break;

                        case "INPUTFILE":
                            inputFile = str2;
                            break;

                        case "OUTPUTFILE":
                            outputFile = str2;
                            break;
                    }
                }
            }
        }

        public static void SaveSettings()
        {
            string[] contents = new string[] { "SUPPLIERID:" + SupplierID, "FTPHOST:" + ftpHost, "FTPUSER:" + ftpUser, "FTPPASS:" + ftpPass, "INPUTFILE:" + inputFile, "OUTPUTFILE:" + outputFile };
            try
            {
                File.WriteAllLines(settingsFile, contents);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
