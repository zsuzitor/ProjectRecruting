﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain.ManyToMany;
using ProjectRecruting.Models.ShortModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Company
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Не указано имя")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Number { get; set; }
        public string Email { get; set; }

        public string ImagePath { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        //public string Image { get; set; }

        //люди которые могут изменять компанию
        public List<CompanyUser> CompanyUsers { get; set; }
        public List<Project> Projects { get; set; }

        public Company()
        {
            //Image = null;
            CompanyUsers = new List<CompanyUser>();
            Projects = new List<Project>();
        }
        public Company(string name, string description, string number, string email)
        {
            Name = name;
            Description = description;
            Number = number;
            Email = email;

        }

        public void ChangeData(string name, string description, string number, string email)//,string image)
        {
            Name = name;
            Description = description;
            Number = number;
            Email = email;
            //if (image != null)
            //    Image = image;

        }

        public async static Task<Company> Get(ApplicationDbContext db, int id)
        {
            return await db.Companys.FirstOrDefaultAsync(x1 => x1.Id == id);
        }
        public async static Task<Company> GetIfAccess(ApplicationDbContext db, string userId, int companyId)
        {
            //var companyUser = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == companyId && x1.UserId == userId);
            //return await db.Companys.FirstOrDefaultAsync(x1 => x1.Id == companyId);
            return await db.Companys.Join(db.CompanyUsers.
                Where(x1 => x1.CompanyId == companyId && x1.UserId == userId&&x1.Status== StatusInCompany.Moderator),
                x1 => x1.Id, x2 => x2.CompanyId, (x1, x2) => x1).FirstOrDefaultAsync();

        }
        public async static Task<bool> CheckAccess(ApplicationDbContext db, string userId, int companyId)
        {
            //var companyUser = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == companyId && x1.UserId == userId);
            //return await db.Companys.FirstOrDefaultAsync(x1 => x1.Id == companyId);
            return (await 
                db.CompanyUsers.
                Where(x1 => x1.CompanyId == companyId && x1.UserId == userId && x1.Status == StatusInCompany.Moderator).CountAsync())>0;

        }

        public async Task SetImage(ApplicationDbContext db,IFormFile[] uploadedFile, IHostingEnvironment appEnvironment)
        {
            if (uploadedFile == null || uploadedFile.Length == 0)
                return;
            var fileName = ContentDispositionHeaderValue.Parse(uploadedFile[0].ContentDisposition).FileName.Trim('"');
            //получаем формат
            var FileExtension = Path.GetExtension(fileName);
            string shortPath = "/images/uploads/company_" + this.Id + "_mainimage" + FileExtension;
            string path = appEnvironment.WebRootPath + shortPath;// + uploadedFile[0].FileName; #TODO формат файла

            bool created=await Image.CheckAndCreate(uploadedFile, path);
            if(created)
            {
                this.ImagePath = shortPath;
                await db.SaveChangesAsync();
            }
            //if (uploadedFile != null && uploadedFile.Length > 0)
            //{
                
            //    // сохраняем файл в папку Files в каталоге wwwroot
            //    using (var fileStream = new FileStream(appEnvironment.WebRootPath + path, FileMode.Create))
            //    {
            //        await uploadedFile[0].CopyToAsync(fileStream);
            //    }
            //}
        }

        //составляем запрос
        public static IQueryable<Company> GetActualQueryEntity(ApplicationDbContext db, int? townId)
        {
            //return db.ProjectUsers.GroupBy(x1 => x1.ProjectId).Select(x1 => new { x1.Key, Count = x1.Count() }).
            //    Join(db.ProjectTowns.Where(x1 => townId == null ? true : x1.TownId == townId), x1 => x1.Key, x2 => x2.ProjectId, (x1, x2) => x1).
            //    Join(db.Projects, x1 => x1.Key, x2 => x2.Id, (x1, x2) => new { companyId = x2.CompanyId, count = x1.Count }).
            //    OrderBy(x1 => x1.count).Join(db.Companys, x1 => x1.companyId, x2 => x2.Id, (x1, x2) => x2);

            //var g = _db.ProjectTowns.Where(x1 => townId == null ? true : (x1.TownId == townId)).
            //    GroupJoin(_db.ProjectUsers, x1 => x1.ProjectId, x2 => x2.ProjectId, (x, y) => new { proj = x, prUser = y }).
            //    SelectMany(x => x.prUser.DefaultIfEmpty(), (x, y) => new { x.proj.ProjectId, y.Id }).GroupBy(x1 => x1.ProjectId).
            //    Join(_db.Projects, x1 => x1.Key, x2 => x2.Id, (x1, x2) => new { companyId = x2.CompanyId, count = x1.Count() }).GroupBy(x1 => x1.companyId).
            //    Select(x1 => new { x1.Key, count = x1.Sum(x2 => x2.count) }).Join(_db.Companys, x1 => x1.Key, x2 => x2.Id, (x1, x2) => new { x1.count, company = x2 }).
            //    OrderBy(x1=>x1.count).Select(x1=>x1.company).ToList();


            var idWithCount = db.ProjectTowns.Where(x1 => townId == null ? true : (x1.TownId == townId)).
              GroupJoin(db.ProjectUsers, x1 => x1.ProjectId, x2 => x2.ProjectId, (x, y) => new { proj = x, prUser = y }).
              SelectMany(x => x.prUser.DefaultIfEmpty(), (x, y) => new { x.proj.ProjectId, y.Id }).GroupBy(x1 => x1.ProjectId).
              Join(db.Projects, x1 => x1.Key, x2 => x2.Id, (x1, x2) => new { companyId = x2.CompanyId, count = x1.Count() }).
              GroupBy(x1 => x1.companyId).Select(x1 => new { x1.Key, count = x1.Sum(x2 => x2.count) });//.ToList();
            return db.Companys.GroupJoin(idWithCount, x1 => x1.Id, x2 => x2.Key, (x, y) => new { company = x, lists = y }).
                           SelectMany(x => x.lists.DefaultIfEmpty(), (x, y) => new { x.company, count = (y == null ? 0 : y.count) }).
                           OrderByDescending(x1=>x1.count).Select(x1=>x1.company);
        }

        //получаем полные данные
        public async static Task<List<CompanyShort>> GetActualEntity(ApplicationDbContext db, int? townId)
        {
            return await Company.GetActualQueryEntity(db, townId).Select(x1=>new CompanyShort(x1)).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }

        //все проекты не зависимо от статуса
        public async static Task<List<ProjectShort>> GetProjectsByActual(ApplicationDbContext db, int companyId, int? townId)
        {
            var res= await Project.GetActualQueryEntity(db, townId).Where(x1 => x1.CompanyId == companyId).Select(x1 => new ProjectShort(x1.Name, x1.Id)).ToListAsync();
            await ProjectShort.SetMainImages(db,res);
            return res;
        }

        public async Task<bool> AddHeadUser(ApplicationDbContext db, string userId)
        {
            var userRelation = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.UserId == userId);
            if (userRelation == null)
                return false;
            if (userRelation.Status == StatusInCompany.Employee || userRelation.Status == StatusInCompany.RequestedByUser)
            {
                userRelation.Status = StatusInCompany.Moderator;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteHeadUser(ApplicationDbContext db, string userId)
        {
            var userRelation = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.UserId == userId && x1.Status == StatusInCompany.Moderator);
            if (userRelation == null)
                return false;
            //db.CompanyUsers.Remove(userRelation);
            userRelation.Status = StatusInCompany.Empty;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddRequest(ApplicationDbContext db, string userId, StatusInCompany status)
        {
            if (status != StatusInCompany.RequestedByCompany && status != StatusInCompany.RequestedByUser)
                return false;
            if(string.IsNullOrWhiteSpace(userId))
                return false;
            var userRelation = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.UserId == userId);
            if(userRelation?.Status== StatusInCompany.Empty)
            {
                userRelation.Status = status;
                await db.SaveChangesAsync();
                return true;
            }
            if (userRelation == null)
            {
                db.CompanyUsers.Add(new CompanyUser(userId,this.Id,status));
                await db.SaveChangesAsync();
                return true;
            }
           
                return false;
        }
        


    }
}
