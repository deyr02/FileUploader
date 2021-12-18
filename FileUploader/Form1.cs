using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;


using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileUploader
{
    public partial class FileUploader : Form
    {
        public FileUploader()
        {
            InitializeComponent();
            txt_fileName.Enabled = false;
        }

        CloudStorageAccount storageAccount = null;
        CloudBlobContainer cloudBlobContainer = null;

        private void btn_select_Click(object sender, EventArgs e)
        {
            //hide the upload panel, incase it is visible
            uploadPanel.Visible = false;
            
            //opening file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = false;
            //filtering files (only csv & xlsx)
            openFileDialog.Filter = "Excel files (*.csv or *.xlsx)|*.csv;*.xlsx";


            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MessageBox.Show(openFileDialog.FileName);

                if (openFileDialog.FileName != "")
                {
                    txt_fileName.Text = Path.GetFileName(openFileDialog.FileName);
                    uploadPanel.Visible = true;
                    //  uploadToFTP(openFileDialog.FileName.ToString());

                    uploadToAzure(openFileDialog.FileName.ToString());
                    
                }
                else
                {
                   

                }

                foreach (string fileName in openFileDialog.FileNames)
                {
                    //Process.Start(fileName);
                }
            }
        }


        private void uploadToFTP(string filePath)
        {
            //using (var ftpServer = new WebClient())
            //{
            //    ftpServer.Credentials = new NetworkCredential("b3_30610330", "Sh@rpC0d3r");

            //   ftpServer.UploadFile( new Uri("ftp://ftp.byethost3.com/htdocs"), WebRequestMethods.Ftp.UploadFile, filePath);
            //} 

            //using (WebClient webClient = new WebClient())
            //{
            //    webClient.Credentials = new NetworkCredential("b3_30610330", "Sh@rpC0d3r");
            //    await webClient.UploadFileTaskAsync(new Uri("ftp://ftp.byethost3.com/htdocs"), WebRequestMethods.Ftp.UploadFile, filePath);
            //}

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", "ftp://ftp.byethost3.com/htdocs", Path.GetFileName(filePath))));

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential("b3_30610330", "Sh@rpC0d3r");

            Stream ftpStream = request.GetRequestStream();
            FileStream fs = File.OpenRead(filePath);
            byte[] buffer = new byte[32];
            double total = (double)fs.Length;
            int byteRead = 0;
            double read = 0;
            do
            {
                byteRead = fs.Read(buffer);
                ftpStream.Write(buffer, 0, byteRead);
                read += (double)byteRead;
                double percentage = read / total * 100;
            } while (byteRead != 0);
            fs.Close();
            ftpStream.Close();



        }


        private async void uploadToAzure(string filePath)

        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=fccdemo1;AccountKey=mCSLo6Lz3rn3INbO69orMHhhgn29nqiKbtEATrDP71OCPiiNf3KIIKJkS0+bXaaT3fKivqBRzj6d5FLhFWpMTg==;EndpointSuffix=core.windows.net";

            //string storageConnection = CloudConfigurationManager.GetSetting("DefaultEndpointsProtocol=https;AccountName=fccdemo1;AccountKey=mCSLo6Lz3rn3INbO69orMHhhgn29nqiKbtEATrDP71OCPiiNf3KIIKJkS0+bXaaT3fKivqBRzj6d5FLhFWpMTg==;EndpointSuffix=core.windows.net"); 
            //CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageConnection);



            ////create a block blob
            //CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            ////create a container
            //CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("appcontainer");

            ////create a container if it is not already exists

            //if (await cloudBlobContainer.CreateIfNotExistsAsync())
            //{

            //    await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            //}

            ////string imageName = "Test-" + Path.GetExtension(openFileDialog.FileName);

            ////get Blob reference

            //CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(Path.GetFileName(filePath)); 
            ////cloudBlockBlob.Properties.ContentType = openFileDialog.ContentType;

            //await cloudBlockBlob.UploadFromFileAsync(filePath);

            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {

                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
               
               
                cloudBlobContainer = cloudBlobClient.GetContainerReference("fccstorage");
               
                //if (cloudBlobContainer != null)
                //{
                //    MessageBox.Show("Null");
                //    return;
                //}
                //if the container do not exists create it. 
               // cloudBlobContainer = cloudBlobClient.GetContainerReference("fccstorage" + Guid.NewGuid().ToString());
              //  await cloudBlobContainer.CreateAsync();
              

                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloudBlobContainer.SetPermissionsAsync(permissions);
            }

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(Path.GetFileName(filePath));
            await cloudBlockBlob.UploadFromFileAsync(filePath);


        }
    }
}
