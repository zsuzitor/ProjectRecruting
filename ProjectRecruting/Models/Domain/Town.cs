using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Town
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public List<ProjectTown> ProjectTowns { get; set; }
        public Town()
        {

        }
    }
}
