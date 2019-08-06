using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain.ManyToMany
{
    public class ProjectUser
    {
        public int Id { get; set; }

        public StatusInProject Status { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

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

        public async Task<bool> ChangeStatus(ApplicationDbContext db, StatusInProject newStatus)
        {
            try
            {
                this.Status = newStatus;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return false;
            }
            return true;
        }
    }
}
