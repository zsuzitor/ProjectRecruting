using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public CompetenceProject()
        {

        }

        public CompetenceProject(int competenceId,int projectId):this()
        {
            CompetenceId= competenceId;
            ProjectId = projectId;
        }
    }
}
