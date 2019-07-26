using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain.ManyToMany
{
    public class CompetenceProject
    {
        public int Id { get; set; }

        public int CompetenceId { get; set; }
        public Competence Competence { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }


        public CompetenceProject()
        {

        }
    }
}
