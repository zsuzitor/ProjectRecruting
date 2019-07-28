using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain.ManyToMany
{
    public class ProjectUser
    {
        public int Id { get; set; }

        public StatusInProject Status { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public ProjectUser()
        {
            Status = StatusInProject.InProccessing;
        }

        public ProjectUser(string userId,int projectId):this()
        {
            UserId = userId;
            ProjectId = projectId;

        }

        public ProjectUser(string userId, int projectId, StatusInProject status) : this(userId,projectId)
        {
            Status = status;
        }
    }
}
