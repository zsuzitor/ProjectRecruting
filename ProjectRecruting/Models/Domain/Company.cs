using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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

        //public string Image { get; set; }

        //люди которые могут изменять компанию
        public List<CompanyUser> CompanyUsers { get; set; }
        public List<Project> Projects { get; set; }

        public Company()
        {
            //Image = null;
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

        public async static Task<Company> GetIfAccess(ApplicationDbContext db, string userId, int companyId)
        {
            //var companyUser = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == companyId && x1.UserId == userId);
            //return await db.Companys.FirstOrDefaultAsync(x1 => x1.Id == companyId);
            return await db.Companys.Join(db.CompanyUsers.
                Where(x1 => x1.CompanyId == companyId && x1.UserId == userId), x1 => x1.Id, x2 => x2.CompanyId, (x1, x2) => x1).FirstOrDefaultAsync();

        }

        public async Task SetImage(IFormFile[] uploadedFile, IHostingEnvironment appEnvironment)
        {
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                string path = "/images/uploads/company_" + this.Id + "_mainimage";// + uploadedFile[0].FileName;
                // сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile[0].CopyToAsync(fileStream);
                }
            }
        }


        public async static Task<List<ProjectShort>> GetProjectsByActual(ApplicationDbContext db, int companyId, int? townId)
        {
            var res= await Project.GetActualQueryEntityInTown(db, townId).Where(x1 => x1.CompanyId == companyId).Select(x1 => new ProjectShort(x1.Name, x1.Id)).ToListAsync();
            await ProjectShort.SetMainImages(db,res);
            return res;
        }

        public async Task<bool> AddHeadUser(ApplicationDbContext db, string userId)
        {
            var userRelation = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.UserId == userId);
            if (userRelation != null)
                return false;
            db.CompanyUsers.Add(new Models.Domain.ManyToMany.CompanyUser(userId, this.Id));
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteHeadUser(ApplicationDbContext db, string userId)
        {
            var userRelation = await db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.UserId == userId);
            if (userRelation == null)
                return false;
            db.CompanyUsers.Remove(userRelation);
            await db.SaveChangesAsync();
            return true;
        }

    }
}
