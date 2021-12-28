using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
        public AzureStorage() { }

        public string ConnectionString { get; set; }

        public string AccountName { get; set; }

        public string ContainerName { get; set; }

        public ContainerType ContainerType { get; set; }

        public bool ConnectionStatus { get; set; }
        public bool IsSelectedConnection { get; set; }


        public   List<FileDetails> ReadFilesFromAzure()
        {
            if (this.ContainerType == ContainerType.FileShare)
            {
                return ReadFromAzureFileShare();
            }
            else
            {
                return  ReadFromAzureBlob();
            }


        }

        public string DownloadFileFromAzure(string azureSourceFileName)
        {
            if (this.ContainerType == ContainerType.FileShare)
            {
                return DownloadFileFromFileShare(azureSourceFileName);
            }
            return DownloadFileFromBlob(azureSourceFileName);

        }


        public string DeleteFileFormAzure(string azureSourceFileName)
        {
            if (this.ContainerType == ContainerType.FileShare)
            {
                return DeleteFileFromFileShare(azureSourceFileName);
            }
            return DeleteFileFromBlob(azureSourceFileName);
        }


        #region Azure File Share


        //Read file form File share conatainer      
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
                    //skipping the directory
                    //Reading directory is not required.
                    if(!item.IsDirectory)
                        fileDetailList.Add(new FileDetails(item.Name, (long)item.FileSize, item.Name.Contains("xlsx") ? "xlsx" : "csv"));
                }
            }

            return fileDetailList;
        }

     
        //Downloadfile form
        //if file download correctly then return downloaded filepath 
        // if any error or exception happen it will return empty string ("")
        private string DownloadFileFromFileShare (string azureSourceFileName)
        {

            try
            {

                string baseURL = @"C:\FileUploader\downloads";
                string downLoadedFilePath = baseURL + @"\" + azureSourceFileName;
                //if the folder is not exist creae one.
                if (!Directory.Exists(baseURL)) { Directory.CreateDirectory(baseURL); }

                //Naming file to avoid overwrite the file;
                //file wriet as A (1) .xlsx, A(2).xlsx etc 
                int fileNameCounter = 1;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(azureSourceFileName);
                string fileExtension = Path.GetExtension(azureSourceFileName);
                if (File.Exists(downLoadedFilePath))
                {
                    while (File.Exists(downLoadedFilePath))
                    {
                        
                        
                        downLoadedFilePath = baseURL + @"\" + fileNameWithoutExtension + string.Format(" ({0})", fileNameCounter) + "." + fileExtension;
                        fileNameCounter++;
                    }

                }
                
                ShareClient share = new ShareClient(this.ConnectionString, this.ContainerName);
                ShareDirectoryClient directory = share.GetRootDirectoryClient();
                ShareFileClient file = directory.GetFileClient(azureSourceFileName);

                // Download the file
                ShareFileDownloadInfo download = file.Download();
                //writing file to the given path.
                using (FileStream stream = File.OpenWrite(downLoadedFilePath))
                {
                    download.Content.CopyTo(stream);
                }
                //return the file path  where file is downloaded 
                return downLoadedFilePath;
            }
            catch (Exception)
            {
                return "";
            }
        }


        /// <summary>
        /// Delaeting file from Azure file Share
        /// 
        /// </summary>
        /// <param name="azureSourceFileName"></param>
        /// <returns>
        /// if FIle is deleted then it will return response string.
        /// if any error exception happen then it will retun empty string. 
        /// </returns>
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
        #endregion

        #region Blob 
        private List<FileDetails> ReadFromAzureBlob()
        {
            List<FileDetails> output = new List<FileDetails>();
            try
            {
                var blobServiceClient = new BlobServiceClient(this.ConnectionString);

                //get container
                var container = blobServiceClient.GetBlobContainerClient(this.ContainerName);

                //Enumerating the blobs may make multiple requests to the service while fetching all the values
                //Blobs are ordered lexicographically by name
                //if you want metadata set BlobTraits - BlobTraits.Metadata
                var blobHierarchyItems = container.GetBlobsByHierarchy(BlobTraits.None, BlobStates.None, "/");

                foreach (var blobHierarchyItem in blobHierarchyItems)
                {
                    //check if the blob is a virtual directory.
                    if (!blobHierarchyItem.IsPrefix)
                    {
                        output.Add(new FileDetails(blobHierarchyItem.Blob.Name, (long)blobHierarchyItem.Blob.Properties.ContentLength, blobHierarchyItem.Blob.Name.Contains("xlsx") ? "xlsx" : "csv"));
                    }
                }
                return output;
            }
            catch (Exception)
            {
                throw new Exception("Error. Something went worng. Please check the connection details");
            }

        }

        private string DownloadFileFromBlob(string azureSourceFileName)
        {

            try
            {

                string baseURL = @"C:\FileUploader\downloads";
                string downLoadedFilePath = baseURL + @"\" + azureSourceFileName;
                //if the folder is not exist creae one.
                if (!Directory.Exists(baseURL)) { Directory.CreateDirectory(baseURL); }

                //Naming file to avoid overwrite the file;
                //file wriet as A (1) .xlsx, A(2).xlsx etc 
                int fileNameCounter = 1;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(azureSourceFileName);
                string fileExtension = Path.GetExtension(azureSourceFileName);
                if (File.Exists(downLoadedFilePath))
                {
                    while (File.Exists(downLoadedFilePath))
                    {


                        downLoadedFilePath = baseURL + @"\" + fileNameWithoutExtension + string.Format(" ({0})", fileNameCounter) + fileExtension;
                        fileNameCounter++;
                    }

                }
                    new BlobClient(this.ConnectionString, this.ContainerName, azureSourceFileName).DownloadTo(downLoadedFilePath);

           
                //return the file path  where file is downloaded 
                return downLoadedFilePath;
            }
            catch (Exception)
            {
                return "";
            }
        }


        private string DeleteFileFromBlob(string azureSourceFileName)
        {
            try
            {
               var response = new BlobClient(this.ConnectionString, this.ContainerName, azureSourceFileName).Delete();

                return response.ToString();
            }
            catch (Exception)
            {
                return "";
            }

        }

        #endregion

    }


}
