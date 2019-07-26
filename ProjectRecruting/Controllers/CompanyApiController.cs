using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain;

namespace ProjectRecruting.Controllers
{
    [Route("api/Company")]
    [ApiController]
    public class CompanyApiController : ControllerBase
    {
       readonly ApplicationDbContext _db = null;
        public CompanyApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public async Task<Company> CreateCompany(Company company, IFormFile uploadedFile)
        {
           
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 404;
                return null;
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                Response.StatusCode = 404;
                return null;
            }

            Company newCompany = new Company(company.Name, company.Description, company.Number, company.Email);
            if (uploadedFile != null)
            
                using (var binaryReader = new BinaryReader(uploadedFile.OpenReadStream()))
            {
                newCompany.Image = binaryReader.ReadBytes((int)uploadedFile.Length);
            }

            _db.Companys.Add(newCompany);
            await _db.SaveChangesAsync();

            _db.CompanyUsers.Add(new Models.Domain.ManyToMany.CompanyUser(userId,newCompany.Id));
            await _db.SaveChangesAsync();
            return newCompany;
        }


        [Authorize]
        public async Task<Company> ChangeCompany(Company company, IFormFile uploadedFile)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;


            if (company.Id == 0)
                ModelState.AddModelError("Id", "Не передан Id");
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 404;
                return null;
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                Response.StatusCode = 404;
                return null;
            }

            var oldCompany=await _db.Companys.FirstOrDefaultAsync(x1=>x1.Id==company.Id);
            if(oldCompany==null)
            {
                Response.StatusCode = 404;
                return null;
            }
            byte[] newImage = null;
            if (uploadedFile != null)
            
                using (var binaryReader = new BinaryReader(uploadedFile.OpenReadStream()))
            {
                    newImage = binaryReader.ReadBytes((int)uploadedFile.Length);
            }
            oldCompany.ChangeData(company.Name, company.Description, company.Number, company.Email, company.Image);
            await _db.SaveChangesAsync();
            return oldCompany;
        }


        [Authorize]
        public async Task<Project> CreateProject(Project project, IFormFileCollection uploads)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 404;
                return null;
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                Response.StatusCode = 404;
                return null;
            }
            //var company=await _db.Companys.FirstOrDefaultAsync(x1=>x1.Id==project.CompanyId);
            //if(company==null)
            //{
            //    Response.StatusCode = 404;
            //    return null;
            //}
            var admins=await _db.CompanyUsers.FirstOrDefaultAsync(x1=>x1.UserId==userId&&x1.CompanyId==project.CompanyId);
            if (admins == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            Project newProject = new Project(project.Name, project.Description, project.Payment, project.CompanyId);
            newProject.SetImages(uploads);
            _db.Projects.Add(newProject);
            await _db.SaveChangesAsync();
            return newProject;


        }




    }
}