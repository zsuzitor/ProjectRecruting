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
        public byte[] Data { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public Image()
        {
            Data = null;
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
    }
}
