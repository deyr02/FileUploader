using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileUploader
{
    public static class ObjectConverter
    {
        public static string SerializeToBase64String(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static T DeserializeToObject<T>(this string base64String)
        {
            return JsonConvert.DeserializeObject<T>(base64String);
        }
    }
}
