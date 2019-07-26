using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Competence
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public List<CompetenceProject> CompetenceProjects { get; set; }

        public Competence()
        {

        }
    }
}
