using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.ResultModel
{
    public class ProjectPage
    {
        public int Id { get; set; }
       
        public string Name { get; set; }
        public string Description { get; set; }

        public bool? Payment { get; set; }

        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public StatusProject Status { get; set; }

        public List<CompetenceShort> Competences { get; set; }

        public List<string> Towns { get; set; }
        public List<int> ImagesId { get; set; }


        public ProjectPage()
        {

        }

        public async static Task<ProjectPage> LoadAllForView(ApplicationDbContext db,Project project)
        {
            if (project == null)
                return null;
            var res = new ProjectPage() {
                Id=project.Id,
                Name = project.Name,
                Description = project.Description,
                Payment = project.Payment,
                CompanyId = project.CompanyId,
                Status = project.Status,
            };

            //#TODO вынести по методам
            res.CompanyName=(await Company.Get(db,res.CompanyId)).Name;
            res.Competences=await db.CompetenceProjects.Where(x1=>x1.ProjectId==res.Id).
                Join(db.Competences,x1=>x1.CompetenceId,x2=>x2.Id,(x1,x2)=> new CompetenceShort(x2)).ToListAsync();

            res.Towns=await db.ProjectTowns.Where(x1=>x1.ProjectId==res.Id).
                Join(db.Towns, x1 => x1.TownId, x2 => x2.Id, (x1, x2) => x2.Name).ToListAsync();

            res.ImagesId = await db.Images.Where(x1=>x1.ProjectId==res.Id).Select(x1=>x1.Id).ToListAsync();

        }


    }
}
