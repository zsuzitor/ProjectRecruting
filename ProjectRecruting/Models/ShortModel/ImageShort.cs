using ProjectRecruting.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.ShortModel
{
    public class ImageShort
    {
        public int Id { get; set; }
        public string Path { get; set; }

        public ImageShort()
        {

        }

        public ImageShort(Image img)
        {
            Id = img.Id;
            Path = img.Path;
        }
    }
}
