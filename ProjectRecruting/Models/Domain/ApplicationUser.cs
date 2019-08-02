﻿using Microsoft.AspNetCore.Identity;
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


        public int RefreshTokenHash { get; set; }

        public List<CompanyUser> CompanyUsers { get; set; }

        public List<ProjectUser> ProjectUsers { get; set; }

        public ApplicationUser()
        {

        }


        public async Task SetRefreshToken(ApplicationDbContext db, string token)
        {
            this.RefreshTokenHash = token.GetHashCode();
            await db.SaveChangesAsync();
        }

        //получить по логину и паролю
        public async static Task<ApplicationUser> LoginGet(UserManager<ApplicationUser> userManager, string userName, string password)
        {
            var user = await userManager.FindByEmailAsync(userName);
            if (user == null)
                return null;
            var passwordOK = await userManager.CheckPasswordAsync(user, password);
            if (!passwordOK)
                return null;
            return user;

        }

        public async Task LoadProjectUsers(ApplicationDbContext db)
        {
            if (!db.Entry(this).Collection(x1 => x1.ProjectUsers).IsLoaded)
                await db.Entry(this).Collection(x1 => x1.ProjectUsers).LoadAsync();
        }

        //проверка на то может ли пользователь редактировать компанию
        public async Task<bool> CheckAccessEditCompany(ApplicationDbContext db, int companyId)
        {
            return await ApplicationUser.CheckAccessEditCompany(db, companyId, this.Id);
        }
        //проверка на то может ли пользователь редактировать компанию
        public async static Task<bool> CheckAccessEditCompany(ApplicationDbContext db, int companyId, string userId)
        {
            //var res = await db.Entry(this).Collection(x1 => x1.CompanyUsers).Query().FirstOrDefaultAsync(x1 => x1.CompanyId == companyId);
            //return res == null ? false : true;
            var res = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == companyId && x1.UserId == userId);
            return res == null ? false : true;

        }
        public async static Task<bool> CheckAccessEditProject(ApplicationDbContext db, Project project, string userId)
        {
            if (project == null)
                return false;
            var res = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == project.CompanyId && x1.UserId == userId);
            return res == null ? false : true;

        }
        public async static Task<Project> CheckAccessEditProject(ApplicationDbContext db, int projectId, string userId)
        {
            var project = await db.Projects.FirstOrDefaultAsync(x1 => x1.Id == projectId);
            if (project == null)
                return null;
            var companyUsers = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == project.CompanyId && x1.UserId == userId);
            return companyUsers == null ? null : project;

        }


        public async static Task<List<UserShort>> GetShortsData(ApplicationDbContext db, List<string> userIds)
        {
            return await db.Users.Where(x1 => userIds.Contains(x1.Id)).Select(x1 => new UserShort(x1.Email, x1.Id)).ToListAsync();
        }

        //компании пользователя
        public async static Task<List<Company>> GetUserCompanys(ApplicationDbContext db, string userId)
        {
            return await db.CompanyUsers.Where(x1 => x1.UserId == userId).Join(db.Companys, x1 => x1.CompanyId, x2 => x2.Id, (x1, x2) => x2).ToListAsync();
        }

        //проекты которые пользователь может редактировать, не по актуальности
        public async static Task<List<Project>> GetUserResponsibilityProjects(ApplicationDbContext db, string userId)
        {
            return await db.CompanyUsers.Where(x1 => x1.UserId == userId).Join(db.Projects, x1 => x1.CompanyId, x2 => x2.Id, (x1, x2) => x2).ToListAsync();

        }

        //без сортировки по актуальности
        public async static Task<List<Project>> GetUserRequests(ApplicationDbContext db, string userId, StatusInProject statusInProject)
        {
            return await db.ProjectUsers.Where(x1 => x1.UserId == userId && x1.Status == statusInProject).
                Join(db.Projects, x1 => x1.ProjectId, x2 => x2.Id, (x1, x2) => x2).ToListAsync();

        }


    }
}
