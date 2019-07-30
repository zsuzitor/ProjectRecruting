using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain.ManyToMany
{
    public class ProjectTown
    {
        public int Id { get; set; }

        public int TownId { get; set; }
        public Town Town { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public ProjectTown()
        {

        }

        public ProjectTown(int townId,int projectId)
        {
            TownId = townId;
            ProjectId = projectId;
        }
    }
}
