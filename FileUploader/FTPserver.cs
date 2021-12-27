using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileUploader
{
    class FTPserver
    {
        public FTPserver(string siteName, string url, string username, string password, bool connectionStatus)
        {
            this.SiteName = siteName;
            this.URL = url;
            this.UserName = username;
            this.Password = password;
            this.ConnectionStatus = connectionStatus;
        }

        public string SiteName { get; set; }
        public string URL { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool ConnectionStatus { get; set; }

        public List<FileDetails> ReadFilesFromFTP
        {
            get
            {
                List<FileDetails> output = new List<FileDetails>();
                var request = (FtpWebRequest)WebRequest.Create(URL);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(UserName, Password);

                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        var reader = new StreamReader(responseStream);

                        while (!reader.EndOfStream)
                        {
                            var lines = reader.ReadLine();
                            if (lines.Contains("xlsx") || lines.Contains("csv"))
                            {
                                output.Add(new FileDetails(lines));
                            }

                        }
                    }
                }
                return output;

            }
        }

        public string DeleteFileFromFTP(string ftpSourceFileName)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", this.URL, ftpSourceFileName)));
                request.Credentials = new NetworkCredential(this.UserName, this.Password);
                request.Method = WebRequestMethods.Ftp.DeleteFile;


                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {

                    return response.StatusDescription;
                }
            }
            catch (Exception)
            {
                return "";
            }
            
        }



        public string DownloadFileFromFTP(string ftpSourceFileName)
        {
            try
            {
                int bytesRead = 0;
                byte[] buffer = new byte[2048];
                string baseURL = @"C:\FileUploader\downloads";
                string downLoadedFilePath = baseURL + @"\" + ftpSourceFileName;
                if (!Directory.Exists(baseURL)) { Directory.CreateDirectory(baseURL); }

                int fileNameCounter = 1;
                if (File.Exists(downLoadedFilePath))
                {
                    while (File.Exists(downLoadedFilePath))
                    {
                        string[] temp = ftpSourceFileName.Split(".");
                        downLoadedFilePath = baseURL + @"\" + temp[0] + string.Format(" ({0})", fileNameCounter) + "." + temp[1];
                        fileNameCounter++;
                    }

                }

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", this.URL, ftpSourceFileName)));
                request.Credentials = new NetworkCredential(this.UserName, this.Password);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                Stream reader = request.GetResponse().GetResponseStream();


                FileStream fileStream = new FileStream(downLoadedFilePath, FileMode.Create, FileAccess.ReadWrite);

                while (true)
                {
                    bytesRead = reader.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                        break;

                    fileStream.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
                return downLoadedFilePath;
            }
            catch (Exception)
            {
                return "";
            }


        }


    }


}
