using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models;
using ProjectRecruting.Models.Domain;

namespace ProjectRecruting.Controllers
{
    [Route("api/Company")]
    [ApiController]
    public class CompanyApiController : ControllerBase
    {
       readonly ApplicationDbContext _db = null;
        readonly UserManager<ApplicationUser> _userManager = null;
        public CompanyApiController(ApplicationDbContext db,UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost]
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
        [HttpPost]
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
        [HttpPost]
        public async Task<Project> CreateProject(Project project, IFormFileCollection uploads,string[]competences)
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
            if (!await ApplicationUser.CheckAccessEditCompany(_db, project.CompanyId, userId))
            {
                Response.StatusCode = 404;
                return null;
            }
            
            Project newProject = new Project(project.Name, project.Description, project.Payment, project.CompanyId);
            _db.Projects.Add(newProject);
            await _db.SaveChangesAsync();
            await newProject.AddImagesToDb(_db,uploads);
            await newProject.AddCompetences(_db,competences);


            return newProject;


        }

        [Authorize]
        [HttpPost]
        public async Task<bool> ChangeProject(Project project, IFormFileCollection uploads,int[] deleteImages, int[] competenceIds)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 404;
                return false;
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                Response.StatusCode = 404;
                return false;
            }
            var oldProj=await ApplicationUser.CheckAccessEditProject(_db,project.Id,userId);
            if (oldProj == null)
            {
                Response.StatusCode = 404;
                return false;
            }
            oldProj.ChangeData(project.Name,project.Description,project.Payment);
            await _db.SaveChangesAsync();

            await oldProj.AddImagesToDb(_db,uploads);
            await Project.DeleteImagesFromDb(_db, oldProj.Id, deleteImages);
            await oldProj.DeleteCompetences(_db, competenceIds);

            return true;

        }


        [Authorize]
        [HttpPost]
        public async Task<bool> CloseProject(int projectId)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);
            if (proj == null)
            {
                Response.StatusCode = 404;
                return false;
            }
            await proj.SetStatus(_db, StatusProject.Closed);
            return true;
        }


        [Authorize]
        [HttpPost]
        public async Task<bool> CompliteProject(int projectId)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);
            if (proj == null)
            {
                Response.StatusCode = 404;
                return false;
            }
            await proj.SetStatus(_db, StatusProject.Complited);
            return true;
        }


        [Authorize]
        [HttpPost]
        public async Task<bool> ApproveStudent(int projectId,string studentId)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);
            if (proj == null)
            {
                Response.StatusCode = 404;
                return false;
            }
            
            return await proj.ChangeStatusUser(_db, StatusInProject.Approved, studentId);
        }

        [Authorize]
        [HttpPost]
        public async Task<bool> CancelStudent(int projectId,string studentId)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);
            if (proj == null)
            {
                Response.StatusCode = 404;
                return false;
            }

            return await proj.ChangeStatusUser(_db, StatusInProject.Canceled, studentId);
        }


        [Authorize]
        public async Task<List<UserShort>> GetStudents(int projectId, string studentId, StatusInProject status)
        {
           
            if (status != StatusInProject.Approved && status != StatusInProject.Canceled && status != StatusInProject.InProccessing)
                return null;

            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            
            var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);

            if (proj == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var ids= await proj.GetStudents(_db, StatusInProject.Approved);
            return await ApplicationUser.GetShortsData(_db,ids);
        }

    }
}