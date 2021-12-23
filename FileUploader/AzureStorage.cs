using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploader
{
    public enum ContainerType { Blob, FileShare };
    class AzureStorage
    {
        public AzureStorage(string accountName, ContainerType containerType, string containerName, string connectionString)
        {
            this.AccountName = accountName;
            this.ContainerType = containerType;
            this.ConnectionString = connectionString;
            this.ContainerName = containerName;
        }

        public string ConnectionString { get; set; }

        public string AccountName { get; set; }

        public string ContainerName { get; set; }

        public ContainerType ContainerType { get; set; }


        public List<FileDetails> ReadFilesFromAzure()
        {
            if (this.ContainerType == ContainerType.FileShare)
            {
                return ReadFromAzureFileShare();
            }
            else
            {
                return null;
            }


        }

        private List<FileDetails> ReadFromAzureFileShare()
        {
            List<FileDetails> fileDetailList = new List<FileDetails>();


            ShareClient shareClient = new ShareClient(this.ConnectionString, this.ContainerName);

            var remaining = new Queue<ShareDirectoryClient>();
            remaining.Enqueue(shareClient.GetRootDirectoryClient());
            while (remaining.Count > 0)
            {
                ShareDirectoryClient dir = remaining.Dequeue();
                foreach (ShareFileItem item in dir.GetFilesAndDirectories())
                {
                    fileDetailList.Add(new FileDetails(item.Name, (long)item.FileSize, item.Name.Contains("xlsx") ? "xlsx" : "csv"));
                }
            }

            return fileDetailList;
        }

        public string DownloadFileFromAzure (string azureSourceFileName )
        {
            if(this.ContainerType == ContainerType.FileShare)
            {
                return DownloadFileFromFileShare(azureSourceFileName);
            }
            return null;

        }

        private string DownloadFileFromFileShare (string azureSourceFileName)
        {

            try
            {

                string baseURL = @"C:\FileUploader\downloads";
                string downLoadedFilePath = baseURL + @"\" + azureSourceFileName;
                if (!Directory.Exists(baseURL)) { Directory.CreateDirectory(baseURL); }

                int fileNameCounter = 1;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(azureSourceFileName);
                string fileExtension = Path.GetExtension(azureSourceFileName);
                if (File.Exists(downLoadedFilePath))
                {
                    while (File.Exists(downLoadedFilePath))
                    {
                        
                        
                        downLoadedFilePath = baseURL + @"\" + fileNameWithoutExtension + string.Format(" ({0}) ", fileNameCounter) + "." + fileExtension;
                        fileNameCounter++;
                    }

                }

                ShareClient share = new ShareClient(this.ConnectionString, this.ContainerName);
                ShareDirectoryClient directory = share.GetRootDirectoryClient();
                ShareFileClient file = directory.GetFileClient(azureSourceFileName);

                // Download the file
                ShareFileDownloadInfo download = file.Download();
                using (FileStream stream = File.OpenWrite(downLoadedFilePath))
                {
                    download.Content.CopyTo(stream);
                }
                return downLoadedFilePath;
            }
            catch (Exception)
            {
                return "";
            }
        }




        public string DeleteFileFormAzure (string azureSourceFileName)
        {
            if (this.ContainerType == ContainerType.FileShare)
            {
                return DeleteFileFromFileShare(azureSourceFileName);
            }
            return null;
        }


        private string DeleteFileFromFileShare (string azureSourceFileName)
        {
            try
            {
                ShareClient share = new ShareClient(this.ConnectionString, this.ContainerName);
                ShareDirectoryClient directory = share.GetRootDirectoryClient();
                ShareFileClient file = directory.GetFileClient(azureSourceFileName);

                // Download the file
                Response response = file.Delete();
                return response.ToString();
            }
            catch (Exception)
            {
                return "";
            }
           
        }
    }


}
