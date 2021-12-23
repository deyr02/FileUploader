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
using System.Threading;
using Azure.Storage.Files.Shares;
using Azure;
using DotLiquid;
using System.Diagnostics;

namespace FileUploader
{
    public partial class FileUploader : Form
    {
        public FileUploader()
        {
            InitializeComponent();
            InitialBind();
        }

        private string FTPURL = "ftp://ftp.byethost3.com/htdocs";
        private string FTPUSERNAME = "b3_30610330";
        private string FTPPASSWORD = "Sh@rpC0d3r";

        private string AZURECONNECTIONSTRING = "DefaultEndpointsProtocol=https;AccountName=fccdemo1;AccountKey=mCSLo6Lz3rn3INbO69orMHhhgn29nqiKbtEATrDP71OCPiiNf3KIIKJkS0+bXaaT3fKivqBRzj6d5FLhFWpMTg==;EndpointSuffix=core.windows.net";
        private string FILESHARECONTAINER = "fccfileshare";

        FTPserver fTPserver;
        AzureStorage azureStorage;
       
        private void InitialBind()
        {
            txt_fileName.Enabled = false;
            tabControl2.TabPages[0].Text = "FTP";
            tabControl2.TabPages[1].Text = "Azure";
            tab_settings.TabPages[0].Text = "FTP";
            tab_settings.TabPages[1].Text = "Azure";
            //RefreshConnection();
            this.HandleCreated += new EventHandler((sender, args) => RefreshConnection());
           // GetFilesFromFTP(FTPURL, FTPUSERNAME, FTPPASSWORD);
            fTPserver = new FTPserver("site_1", FTPURL, FTPUSERNAME, FTPPASSWORD, true);
            LoadDataIntoDataGrid(ftp_data_grid, fTPserver.ReadFilesFromFTP);
            azureStorage = new AzureStorage("azure_site_1", ContainerType.FileShare, FILESHARECONTAINER, AZURECONNECTIONSTRING);
            LoadDataIntoDataGrid(azure_data_grid, azureStorage.ReadFilesFromAzure());
        }

       public void RefreshConnection()
        {
            Ftp_fileTranser_status.Text = "";
            azure_file_transfer_status.Text = "";
            Invoke(new Action(async () => await CheckFTPConnection(FTPURL, FTPUSERNAME, FTPPASSWORD)));
            Invoke(new Action(async () => await CheckFileShareConnection(AZURECONNECTIONSTRING, FILESHARECONTAINER)));
        }

        CloudStorageAccount storageAccount = null;
        CloudBlobContainer cloudBlobContainer = null;

        private string filePath = "";

        private void btn_select_Click(object sender, EventArgs e)
        {

            //opening file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = false;
            //filtering files (only csv & xlsx)
            openFileDialog.Filter = "Excel files (*.csv or *.xlsx)|*.csv;*.xlsx";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {


                if (openFileDialog.FileName != "")
                {
                    txt_fileName.Text = Path.GetFileName(openFileDialog.FileName);

                    filePath = openFileDialog.FileName;
                    BindingFileInfo(openFileDialog.FileName);
                }

            }

            else
            {

            }
        }

      
        private async void uploadToFTP(string filePath)
        {

            try
            {
                Invoke(new Action(() => { Ftp_fileTranser_status.Text = "Connected to FTP server."; }));
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", FTPURL, Path.GetFileName(filePath))));
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                Invoke(new Action(() => { Ftp_fileTranser_status.Text = "Preparing file to transfer to ftp server....."; }));

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

                    Invoke(new Action(() => {
                        Ftp_fileTranser_status.Text = (int)percentage + "% completed.";
                        prog_ftp.Value = (int)percentage;


                    }));

                    Thread.Sleep(10);
                } while (byteRead != 0);
                fs.Close();
                ftpStream.Close();

                Invoke(new Action(() => { Ftp_fileTranser_status.Text = "finalizing the file transfer......"; }));
                Thread.Sleep(100);
                Invoke(new Action(() => { Ftp_fileTranser_status.Text = "File transfered to FTP Server"; }));
                Ftp_fileTranser_status.ForeColor = Color.SeaGreen;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);

                Invoke(new Action(() => {

                    Ftp_fileTranser_status.ForeColor = Color.Red;
                    img_ftp_correct.Visible = false;
                    img_ftp_incorrect.Visible = true;
                    Ftp_fileTranser_status.Text = "File was not transfered. Try again later. ";
                }));
            
            }

           

        }


        private async void uploadToAzureBlob(string filePath)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=fccdemo1;AccountKey=mCSLo6Lz3rn3INbO69orMHhhgn29nqiKbtEATrDP71OCPiiNf3KIIKJkS0+bXaaT3fKivqBRzj6d5FLhFWpMTg==;EndpointSuffix=core.windows.net";

            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                cloudBlobContainer = cloudBlobClient.GetContainerReference("fccstorage");
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloudBlobContainer.SetPermissionsAsync(permissions);
            }

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(Path.GetFileName(filePath));
            await cloudBlockBlob.UploadFromFileAsync(filePath);


        }


        private void uploadToAzureFileShare(string filePath)
        {
            try
            {
                Invoke(new Action(() => { azure_file_transfer_status.Text = "Connecting to Azure File share storage."; }));
                Thread.Sleep(500);

                // Get a reference to a share and then create it
                ShareClient share = new ShareClient(AZURECONNECTIONSTRING, FILESHARECONTAINER);

                // Get a reference to a directory and create it
                ShareDirectoryClient directory = share.GetRootDirectoryClient();


                // Get a reference to a file and upload it
                ShareFileClient file = directory.GetFileClient(Path.GetFileName(filePath));
                Invoke(new Action(() => { azure_file_transfer_status.Text = "Connected to Azure file share storage ."; }));
                Thread.Sleep(100);


                Invoke(new Action(() => { azure_file_transfer_status.Text = "Preparing file to upload"; }));

                using (FileStream stream = File.OpenRead(filePath))
                {
                    file.Create(stream.Length);
                    #region
                    //This block of code just used to show the progress bar.

                    FileStream fs = File.OpenRead(filePath);
                    byte[] buffer = new byte[32];
                    double total = (double)fs.Length;
                    int byteRead = 0;
                    long read = 0;
                    do
                    {
                        byteRead = fs.Read(buffer);
                        read += (long)byteRead;
                        double percentage = read / total * 100;
                        Invoke(new Action(() =>
                        {
                            azure_file_transfer_status.Text = (int)percentage + "% completed.";
                            prog_azure.Value = (int)percentage;
                        }));

                        Thread.Sleep(10);
                    } while (byteRead != 0);
                    fs.Close();
                    #endregion
                    Invoke(new Action(() => { azure_file_transfer_status.Text = "finalizing the file transfer......"; }));
                    file.UploadRange(new HttpRange(0, stream.Length), stream);


                    Invoke(new Action(() => { azure_file_transfer_status.Text = "File transfered to Aure file share"; }));
                    azure_file_transfer_status.ForeColor = Color.SeaGreen;
                }
            }

            catch (Exception e)
            {
                azure_file_transfer_status.ForeColor = Color.Red;
                img_azure_correct.Visible = false;
                img_azure_incorrect.Visible = true;
                azure_file_transfer_status.Text = "File was not transfered. Try again later. ";


            }
            
        }

        private void tabControl2_DrawItem(object sender, DrawItemEventArgs e)
        {
            string tabName = tabControl2.TabPages[e.Index].Text;
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            
            //Find if it is selected, this one will be hightlighted...
            if (e.Index == tabControl2.SelectedIndex)
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
            e.Graphics.DrawString(tabName, this.Font, Brushes.Black, tabControl2.GetTabRect(e.Index), stringFormat);
        }

        private void tab_settings_DrawItem(object sender, DrawItemEventArgs e)
        {
            string tabName = tab_settings.TabPages[e.Index].Text;
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            //Find if it is selected, this one will be hightlighted...
            if (e.Index == tab_settings.SelectedIndex)
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
            e.Graphics.DrawString(tabName, this.Font, Brushes.Black, tabControl2.GetTabRect(e.Index), stringFormat);
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        private void BindingFileInfo(string filePath)
        {

            lbl_filename.Text = Path.GetFileName(filePath);

            FileInfo fileInfo = new FileInfo(filePath);
            double _fileSize = fileInfo.Length / 1024;
            lbl_size.Text = _fileSize.ToString() + " KB";

            lbl_created.Text = fileInfo.CreationTime.ToString();
            lbl_modified.Text = fileInfo.LastWriteTime.ToString();
            lbl_location.Text = filePath;

            picBox_select_file.Visible = true;

            //make sure Ftp file transfer status cleard;
            Ftp_fileTranser_status.Text = "";
            Ftp_fileTranser_status.ForeColor = Color.Black;
            prog_ftp.Value = 0;

            //make sure azure file transfer status cleard;
            azure_file_transfer_status.Text = "";
            azure_file_transfer_status.ForeColor = Color.Black;
            prog_azure.Value = 0;
        }
        private void UnBindingFileInfo()
        {
            lbl_filename.Text = "No file selected.";
            lbl_size.Text = "No file selected.";

            lbl_created.Text = "No file selected.";
            lbl_modified.Text = "No file selected.";
            lbl_location.Text = "No file selected.";
            picBox_select_file.Visible = false;


        }

      

        private void btn_upload_Click(object sender, EventArgs e)
        {
            if (filePath == "")
            {
                MessageBox.Show("Please select file.");
                return;
            }
            if (!chk_FTP.Checked && !chk_AZURE.Checked)
            {
                MessageBox.Show("Please select the uploading destinations.");
                return;
            }
            Thread ftpThread;
            Thread azureThread;
            if (chk_FTP.Checked)
            {
                ftpThread = new Thread(() => uploadToFTP(filePath));

                ftpThread.Start();


            }
            if (chk_AZURE.Checked)
            {
                azureThread = new Thread(() => uploadToAzureFileShare(filePath));

                azureThread.Start();

            }

        }

        private async Task<bool> CheckFTPConnection(string URL, string username, string password)
        {
            try
            {
                ftp_conection_check_loading.Visible = true;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(URL);
                request.Credentials = new NetworkCredential(username, password);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false; // useful when only to check the connection.
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();
                ftp_conection_check_loading.Visible = false;
                img_ftp_correct.Visible = true;
                return true;
            }
            catch (Exception)
            {
                ftp_conection_check_loading.Visible = false;
                img_ftp_incorrect.Visible = true;
                return false;
            }
        }

        private async Task<bool> CheckFileShareConnection(string connectionstring, string fileShareContainer)
        {
            try
            {
                azure_conection_check_loading.Visible = true;

                ShareClient share = new ShareClient(connectionstring, fileShareContainer);
                bool testResult = await share.ExistsAsync();

                if (testResult)
                {
                    azure_conection_check_loading.Visible = false;
                    img_azure_correct.Visible = true;
                }
                return testResult;
                    
            }
            catch (Exception)
            {
                azure_conection_check_loading.Visible = false;
                img_azure_incorrect.Visible = true;
                return false;
            }
            
        }

        private void lnk_refresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            img_ftp_correct.Visible = false;   
            img_ftp_correct.Visible = false;

            img_azure_correct.Visible = false;
            img_azure_incorrect.Visible = false;

            ftp_conection_check_loading.Visible = true;
            azure_conection_check_loading.Visible = true;
            prog_azure.Value = 0;
            prog_ftp.Value = 0;
            RefreshConnection();
        }


       

        private void LoadDataIntoDataGrid(DataGridView gridView, List<FileDetails> fileList)
        {
        
           
            gridView.Columns.Add(new DataGridViewImageColumn());
            gridView.Columns.Add(new DataGridViewTextBoxColumn());
            gridView.Columns.Add(new DataGridViewTextBoxColumn());
            gridView.Columns.Add(new DataGridViewImageColumn());
            gridView.Columns.Add(new DataGridViewImageColumn());
            
            gridView.Columns[0].Width = 40;
            
            gridView.Columns[1].Width = 300;
            gridView.Columns[1].HeaderText = "File Name";
          
            gridView.Columns[2].HeaderText = "File Size";
            gridView.Columns[2].Width = 50;
            
            gridView.Columns[3].Width = 40;
            gridView.Columns[4].Width = 40;


            List<FileDetails> files = fileList;

            for (int i = 0; i < files.Count; i++) 
            {
                //string[] row_data = files[i].FileDetails();
                //ftp_data_grid.Rows.Add(row_data);


                DataGridViewRow r = new DataGridViewRow();
                gridView.Rows.Add(r);
              
                gridView.Rows[i].Cells[0].Value = files[i].FileExtension == "xlsx"? Properties.Resources.xlsx: Properties.Resources.csv;
               
                gridView.Rows[i].Cells[1].Value = files[i].FileInfos()[0];
                gridView.Rows[i].Cells[2].Value = files[i].FileInfos()[1];
               
                gridView.Rows[i].Cells[3].Value = Properties.Resources.trash;
                gridView.Rows[i].Cells[4].Value = Properties.Resources.Download;
                
                gridView.Rows[i].Height = 40;
                
            }

        }
        private void FtpFileDownloadHelper (string fileName)
        {

            Invoke(new Action(() =>
            {
                ftp_loading_tab.Visible = true;
                lbl_ftp_loading_tab_status.ForeColor = Color.Black;
                lbl_ftp_loading_tab_status.Visible = true;
                lbl_ftp_loading_tab_status.Text = "Prepareing to download file";
            }));
            Thread.Sleep(50);

            Invoke(new Action(() =>
            {
                lbl_ftp_loading_tab_status.Text = "Downloading file .....";
            }));
            Thread.Sleep(50);

            string downloadedFilePath = fTPserver.DownloadFileFromFTP(fileName);
            if (downloadedFilePath != "")
            {
                Invoke(new Action(() =>
                {
                    lbl_ftp_loading_tab_status.ForeColor = Color.Green;
                    lbl_ftp_loading_tab_status.Text = "Download completed";
                    ftp_loading_tab.Visible = false;
                    Thread.Sleep(50);
                    lbl_ftp_loading_tab_status.Text = string.Format("Downloaded to : {0}", downloadedFilePath);
                    

                }));
                DialogResult dialogResult = MessageBox.Show(string.Format("{0} has been downloaded. Would you like to open it?", fileName), "File Downloaded", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    new Process
                    {
                        StartInfo = new ProcessStartInfo(downloadedFilePath)
                        {
                            UseShellExecute = true
                        }
                    }.Start();
                }

            }
            else
            {
                Invoke(new Action(() =>
                {
                    lbl_ftp_loading_tab_status.ForeColor = Color.Red;
                    lbl_ftp_loading_tab_status.Text = string.Format("{0} was not downloaded. Please again later", fileName);
                    ftp_loading_tab.Visible = false;
                }));
            }

            

        }
   
        private void FtpFileDeleteHelper (string fileName, int rowIndex)
        {
            DialogResult dialogResult = MessageBox.Show(string.Format("Are you sure?  You want to delete '{0}'", fileName), "Delete file", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Invoke(new Action(() =>
                {
                    ftp_loading_tab.Visible = true;
                    lbl_ftp_loading_tab_status.ForeColor = Color.Red;
                    lbl_ftp_loading_tab_status.Visible = true;
                    lbl_ftp_loading_tab_status.Text = "Prepareing to Delete the selected file";
                }));
                Thread.Sleep(50);

                Invoke(new Action(() =>
                {
                    lbl_ftp_loading_tab_status.Text = "Deleting file .....";
                }));
                Thread.Sleep(50);

                string deleteFile = fTPserver.DeleteFileFromFTP(fileName);
                
                if(deleteFile != "")
                {

                    Invoke(new Action(() =>
                    {
                        lbl_ftp_loading_tab_status.ForeColor = Color.Green;
                        lbl_ftp_loading_tab_status.Text = string.Format("{0} has been deleted successfully.", fileName);
                        ftp_loading_tab.Visible = false;
                        ftp_data_grid.Rows.RemoveAt(rowIndex);
                    }));
                }
                else
                {
                    Invoke(new Action(() =>
                    {
                        lbl_ftp_loading_tab_status.Text = string.Format("File wasn't deleted. Please try again later.");
                        ftp_loading_tab.Visible = false;
                    }));
                }


            }
        }
        private void ftp_data_grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string fileName = ftp_data_grid.Rows[e.RowIndex].Cells[1].Value.ToString();
            if (e.ColumnIndex == 4 && e.RowIndex >=0)
            {
                Thread downloadThread = new Thread(() => FtpFileDownloadHelper(fileName));
                downloadThread.Start();   
            }
            else if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                Thread deleteThread = new Thread(() => FtpFileDeleteHelper(fileName, e.RowIndex));
                deleteThread.Start();
            }
            
        }










        private void AzureFileDownloadHelper(string fileName)
        {

            Invoke(new Action(() =>
            {
                azure_loading_tab.Visible = true;
                lbl_azure_loading_tab_status.ForeColor = Color.Black;
                lbl_azure_loading_tab_status.Visible = true;
                lbl_azure_loading_tab_status.Text = "Prepareing to download file";
            }));
            Thread.Sleep(50);

            Invoke(new Action(() =>
            {
                lbl_azure_loading_tab_status.Text = "Downloading file .....";
            }));
            Thread.Sleep(50);

            string downloadedFilePath = azureStorage.DownloadFileFromAzure(fileName);
            if (downloadedFilePath != "")
            {
                Invoke(new Action(() =>
                {
                    lbl_azure_loading_tab_status.ForeColor = Color.Green;
                    lbl_azure_loading_tab_status.Text = "Download completed";
                    ftp_loading_tab.Visible = false;
                    Thread.Sleep(50);
                    lbl_azure_loading_tab_status.Text = string.Format("Downloaded to : {0}", downloadedFilePath);


                }));
                DialogResult dialogResult = MessageBox.Show(string.Format("{0} has been downloaded. Would you like to open it?", fileName), "File Downloaded", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    new Process
                    {
                        StartInfo = new ProcessStartInfo(downloadedFilePath)
                        {
                            UseShellExecute = true
                        }
                    }.Start();
                }

            }
            else
            {
                Invoke(new Action(() =>
                {
                    lbl_azure_loading_tab_status.ForeColor = Color.Red;
                    lbl_azure_loading_tab_status.Text = string.Format("{0} was not downloaded. Please again later", fileName);
                    azure_loading_tab.Visible = false;
                }));
            }



        }


        private void azure_data_grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string fileName = azure_data_grid.Rows[e.RowIndex].Cells[1].Value.ToString();
            if (e.ColumnIndex == 4 && e.RowIndex >= 0)
            {
                Thread downloadThread = new Thread(() => AzureFileDownloadHelper(fileName));
                downloadThread.Start();
            }
            else if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                Thread downloadThread = new Thread(() => AzureFileDeleteHelper(fileName, e.RowIndex));
                downloadThread.Start();
            }

        }
        private void AzureFileDeleteHelper(string fileName, int rowIndex)
        {
            DialogResult dialogResult = MessageBox.Show(string.Format("Are you sure?  You want to delete '{0}'", fileName), "Delete file", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Invoke(new Action(() =>
                {
                    azure_loading_tab.Visible = true;
                    lbl_azure_loading_tab_status.ForeColor = Color.Red;
                    lbl_azure_loading_tab_status.Visible = true;
                    lbl_azure_loading_tab_status.Text = "Prepareing to Delete the selected file";
                }));
                Thread.Sleep(50);

                Invoke(new Action(() =>
                {
                    lbl_azure_loading_tab_status.Text = "Deleting file .....";
                }));
                Thread.Sleep(50);

                string deleteFile = azureStorage.DeleteFileFormAzure(fileName);

                if (deleteFile != "")
                {

                    Invoke(new Action(() =>
                    {
                        lbl_azure_loading_tab_status.ForeColor = Color.Green;
                        lbl_azure_loading_tab_status.Text = string.Format("{0} has been deleted successfully.", fileName);
                        azure_loading_tab.Visible = false;
                        azure_data_grid.Rows.RemoveAt(rowIndex);
                    }));
                }
                else
                {
                    Invoke(new Action(() =>
                    {
                        lbl_azure_loading_tab_status.Text = string.Format("File wasn't deleted. Please try again later.");
                        azure_loading_tab.Visible = false;
                    }));
                }


            }
        }

    }
}
