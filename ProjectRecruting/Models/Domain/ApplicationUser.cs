using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectRecruting.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string SurName { get; set; }


        public List<CompanyUser> CompanyUsers { get; set; }

        public List<ProjectUser> ProjectUsers { get; set; }

        public ApplicationUser()
        {

        }


        public async Task LoadProjectUsers(ApplicationDbContext db )
        {
            if(!db.Entry(this).Collection(x1 => x1.ProjectUsers).IsLoaded)
            await db.Entry(this).Collection(x1 => x1.ProjectUsers).LoadAsync();
        }

        //проверка на то может ли пользователь редактировать компанию
        public async Task<bool> CheckAccessEditCompany(ApplicationDbContext db,int companyId)
        {
            return await ApplicationUser.CheckAccessEditCompany(db,companyId, this.Id);
        }
        //проверка на то может ли пользователь редактировать компанию
        public async static Task<bool> CheckAccessEditCompany(ApplicationDbContext db, int companyId,string userId)
        {
            //var res = await db.Entry(this).Collection(x1 => x1.CompanyUsers).Query().FirstOrDefaultAsync(x1 => x1.CompanyId == companyId);
            //return res == null ? false : true;
            var res = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == companyId && x1.UserId == userId);
            return res == null ? false : true;

        }
        public async static Task<bool> CheckAccessEditProject(ApplicationDbContext db, Project project, string userId)
        {
            var res = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == project.CompanyId && x1.UserId == userId);
            return res == null ? false : true;

        }
        public async static Task<Project> CheckAccessEditProject(ApplicationDbContext db, int projectId, string userId)
        {
            var project=await db.Projects.FirstOrDefaultAsync(x1=>x1.Id== projectId);
            
            var companyUsers = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == project.CompanyId && x1.UserId == userId);
            return companyUsers == null ? null : project;

        }

    }
}
