using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace CSN_File_Converter
{
    public static class Ftp
    {
        public static bool success;

        public static void SendFile(string sourceFile, string remoteFile)
        {
            success = false;
            if (((settings_info.ftpHost == null) || (settings_info.ftpUser == null)) || (settings_info.ftpPass == null))
            {
                MessageBox.Show("Please fill in the FTP info on the Settings window.");
            }
            else
            {
                string requestUriString = "ftp://" + settings_info.ftpHost + remoteFile;
                byte[] buffer = new byte[0];
                FtpWebRequest request = null;
                try
                {
                    request = (FtpWebRequest)WebRequest.Create(requestUriString);
                    request.Credentials = new NetworkCredential(settings_info.ftpUser, settings_info.ftpPass);
                    request.KeepAlive = true;
                    request.UseBinary = true;
                    request.Method = "STOR";
                    FileStream stream = System.IO.File.OpenRead(sourceFile);
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    stream.Close();
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(buffer, 0, buffer.Length);
                    requestStream.Close();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }
                success = true;
            }
        }
    }
}
