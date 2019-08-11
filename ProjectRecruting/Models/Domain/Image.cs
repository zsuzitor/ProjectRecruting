using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Image
    {
        public int Id { get; set; }
        //public int CompanyId { get; set; }
        //public Company Company { get; set; }
        //public byte[] Data { get; set; }

        public string Path { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public Image()
        {
           // Data = null;
        }

        public static List<byte[]> GetBytes(IFormFileCollection images)
        {
            List<byte[]> res = new List<byte[]>();
            if(images!=null)
            foreach (var i in images)
            {
                //byte[] newImage = null;
                if (i != null)

                    using (var binaryReader = new BinaryReader(i.OpenReadStream()))
                    {
                        res.Add(binaryReader.ReadBytes((int)i.Length));
                    }

            }
            return res;
        }

        //не добавляет в бд
        public async static Task<bool> CheckAndCreate(IFormFile[] uploadedFile, string path)//int count
        {
            if (uploadedFile == null)
                return false;
            if(uploadedFile.Length<1)
                return false;
            //for (var i = 0; i < 1 && i < uploadedFile.Length; ++i)//i<count //////#TODO тут оставил цикл на будущее , сохранится только 1 картинка
            //{
                byte[] bytes = null;
                using (var binaryReader = new BinaryReader(uploadedFile[0].OpenReadStream()))
                {
                    bytes = binaryReader.ReadBytes((int)uploadedFile[0].Length);
                }
                bool isImage = Image.IsImage(bytes);
                if (!isImage)
                return false; //continue;
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await fileStream.WriteAsync(bytes);
                    // await uploadedFile[0].CopyToAsync(fileStream);
               // }
            }
            return true;

        }

        //public async static Task CheckAndCreateDb(IFormFile uploadedFile, string path)
        //{
        //    if (uploadedFile == null)
        //        return;
            
        //        byte[] bytes = null;
        //        using (var binaryReader = new BinaryReader(uploadedFile.OpenReadStream()))
        //        {
        //            bytes = binaryReader.ReadBytes((int)uploadedFile.Length);
        //        }
        //        bool isImage = Image.IsImage(bytes);
        //        if (!isImage)
        //            return;
        //        using (var fileStream = new FileStream(path, FileMode.Create))
        //        {
        //            await fileStream.WriteAsync(bytes);
        //            // await uploadedFile[0].CopyToAsync(fileStream);
        //        }
            

        //}

        public static bool IsImage(byte[] data)
        {
            var dataIsImage = false;
            using (var imageReadStream = new MemoryStream(data))
            {
                try
                {
                    using (var possibleImage = System.Drawing.Image.FromStream(imageReadStream))
                    {
                    }
                    dataIsImage = true;
                }
                // Here you'd figure specific exception to catch. Do not leave like that.
                catch
                {
                    dataIsImage = false;
                }
            }

            return dataIsImage;
        }
    }
}
