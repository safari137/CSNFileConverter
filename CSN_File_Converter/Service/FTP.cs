using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace CSN_File_Converter.Service
{
    public class FtpService
    {
        public bool Success { get; private set; }

        public FtpService()
        {
            Success = false;
        }

        public void SendFile(string sourceFile, string remoteFile)
        {
            if (((SettingsService.FtpHost == null) || (SettingsService.FtpUser == null)) || (SettingsService.FtpPass == null))
            {
                MessageBox.Show("Please fill in the FTP info on the Settings window.");
                return;
            }

            var requestUriString = "ftp://" + SettingsService.FtpHost + remoteFile;
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(requestUriString);
                request.Credentials = new NetworkCredential(SettingsService.FtpUser, SettingsService.FtpPass);
                request.KeepAlive = true;
                request.UseBinary = true;
                request.Method = "STOR";
                var stream = System.IO.File.OpenRead(sourceFile);
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Close();
                var requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }
            Success = true;
        }
    }
}
