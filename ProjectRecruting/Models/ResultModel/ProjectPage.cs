using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain;
using ProjectRecruting.Models.ShortModel;
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

        public bool CanEdit { get; set; }
        public StatusProject Status { get; set; }

        public List<CompetenceShort> Competences { get; set; }

        public List<TownShort> Towns { get; set; }
        public List<ImageShort> Images { get; set; }


        public ProjectPage()
        {
            CanEdit = false;
        }

        public async static Task<ProjectPage> LoadAllForView(ApplicationDbContext db,Project project,string userId)
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

       
            res.CompanyName = await project.GetCompanyName(db);
            //res.CompanyName=(await Company.Get(db,res.CompanyId)).Name;
            res.Competences =await Project.GetCompetences(db,res.Id);
            res.Towns = await Project.GetTownsShort(db,res.Id);
            res.Images = await Project.GetImagesShort(db, res.Id);

            res.CanEdit = await project.CheckAccess(db, userId) ;

            return res;
        }


    }
}
