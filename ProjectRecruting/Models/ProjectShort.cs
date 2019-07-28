using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models
{
    public class ProjectShort
    {
        public string Name { get; set; }
        public int IdProject { get; set; }
        public StatusInProject Status { get; set; }

        public ProjectShort()
        {
            Status = StatusInProject.Not;
        }


        public ProjectShort(string name,int idProject):this()
        {
            Name = name;
            IdProject = idProject;
        }
    }
}
