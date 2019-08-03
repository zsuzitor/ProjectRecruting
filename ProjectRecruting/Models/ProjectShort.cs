using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models
{
    public class ProjectShort
    {
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public StatusInProject Status { get; set; }

        public int? MainImageId { get; set; }
        //public int CompanyId { get; set; }

        public ProjectShort()
        {
            Status = StatusInProject.Not;
            MainImageId = null;
        }


        public ProjectShort(string name, int projectId) : this()
        {
            Name = name;
            ProjectId = projectId;
        }



        public async static Task SetMainImages(ApplicationDbContext db, List<ProjectShort> projects)
        {
            var images = await db.Images.Where(x1 => projects.FirstOrDefault(x2 => x2.ProjectId == x1.ProjectId) != null).
                GroupBy(x1 => x1.ProjectId).Select(x1 => new { projectId = x1.Key, imId = x1.First().Id }).ToListAsync();

            projects.ForEach(x1 =>
            {
                var tmpImg = images.FirstOrDefault(x2 => x2.projectId == x1.ProjectId);
                if (tmpImg != null)
                    x1.MainImageId = tmpImg.imId;

            });

            //db.Images.Join(projects, x1 => x1.ProjectId, x2 => x2.ProjectId, (x1, x2) => new { x1.ProjectId, x1.Id }).ToListAsync();
        }
    }
}
