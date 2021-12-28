using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploader
{
    class AccountHandler
    {

        private string baseURL = @"C:\FileUploader\appData";
        private string ftpURL = @"C:\FileUploader\appData\Don't Delete ftpServer.txt";
        private string azureURL = @"C:\FileUploader\appData\Don't delete azureAccunt.txt";

        private bool IsDirectoryExist()
        {
            //if direcory is not created then create one. 
            if (!Directory.Exists(baseURL))
            {
                Directory.CreateDirectory(baseURL);
            }
            return true;
        }

        private bool IsFileExists(string filePath) => File.Exists(filePath);

        private void Write<T>(List<T> list, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                //writing object by object
                foreach (var obj in list)
                {
                    //seialize the object into string
                    string objString = JsonConvert.SerializeObject(obj);
                    //encoding object string 
                    string encodedStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(objString));
                    //Write to the file
                    sw.WriteLine(encodedStr);
                }
            }

        }

        public void WriteFile<T>(List<T> lists)
        {

            if (IsDirectoryExist())
            {
                Type objectType = typeof(T);

                if (objectType.Name == "FTPserver")
                {
                    Write(lists, ftpURL);
                }
                else
                {
                    Write(lists, azureURL);
                }
            }
        }


        private List<T> Read<T>(string filePath)
        {
            List<T> list = new List<T>();
            //if file path is not existed retun empty
            if (!File.Exists(filePath)) return list;

            //Reading line by line
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    //decoring string
                    string objectString = Encoding.UTF8.GetString(Convert.FromBase64String(line));
                    //deserailize string
                    JsonSerializer JS = new JsonSerializer();
                    object obj = JsonConvert.DeserializeObject<T>(objectString);
                    //convert to types and add to the list
                    list.Add((T)obj);
                }
            }

            return list;
        }



        public List<T> ReadFile<T>()
        {
            //determining the type and comman to read from specific file
            if (typeof(T).Name == "FTPserver")
            {
                return Read<T>(ftpURL);
            }
            else
            {
                return Read<T>(azureURL);
            }
        }
    }
}
