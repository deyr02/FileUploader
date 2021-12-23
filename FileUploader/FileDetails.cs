using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploader
{

    class FileDetails
    {
        public FileDetails(string responseLine)
        {

            string[] details = responseLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);


            this.FileSize = Convert.ToInt64(details[4]);
            string _fileName = "";
            for (int i = 8; i < details.Length; i++)
            {
                if (i == details.Length - 1)
                {
                    _fileName += details[i];
                    this.FileExtension = details[i].Contains("xlsx") ? "xlsx" : "csv";
                }
                else
                {
                    _fileName += details[i] + " ";

                }

            }
            this.FileName = _fileName;
            Console.WriteLine(this.ToString());
        }

        public FileDetails(string fileName, long fileSize, string fileExtension)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
            this.FileExtension = fileExtension;
            Console.WriteLine(this.ToString());
        }

        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public long FileSize { get; set; }


        public override string ToString()
        {
            return string.Format("FileName: {0}, FileSize: {1}, FileType: {2}", this.FileName, this.CalculatingFileSize(), this.FileExtension);
        }

        private string CalculatingFileSize()
        {

            double output = 0;
            string[] fileMeasurement = new string[] { " KB", " MB", " GB", " TB" };
            long devisor = 1024;
            int sizeUnitCounter = 0;
            while (true)
            {
                if (this.FileSize < 1024)
                {
                    output = FileSize;
                    break;
                }
                if (this.FileSize / devisor < 1024)
                {
                    output = this.FileSize / devisor;
                    break;
                }

                if (sizeUnitCounter >= 3)
                {

                    break;

                }

                output = this.FileSize / devisor;
                sizeUnitCounter++;
                devisor *= 1024;
            }

            return output + fileMeasurement[sizeUnitCounter];
        }

        public string[] FileInfos() => new string[] { FileName, this.CalculatingFileSize() };
    }
   
}
