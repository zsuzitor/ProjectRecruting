using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool Payment { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }


        public List<CompetenceProject> CompetenceProjects { get; set; }
        public List<ProjectUser> ProjectUsers { get; set; }
        public List<ProjectTown> ProjectTowns { get; set; }

        public Project()
        {

        }


    }
}
