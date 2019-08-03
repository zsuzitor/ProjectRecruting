using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
        //readonly UserManager<ApplicationUser> _userManager = null;
        IHostingEnvironment _appEnvironment { get; set; }
        public CompanyApiController(ApplicationDbContext db, IHostingEnvironment appEnvironment)// UserManager<ApplicationUser> userManager,
        {
            _db = db;
            //_userManager = userManager;
            _appEnvironment = appEnvironment;
        }

        //[Authorize]
        [HttpPost("CreateCompany")]
        public async Task<string> CreateCompany([FromForm]Company company, [FromForm]IFormFile[] uploadedFile = null)
        {
            //var file = HttpContext.Request.Form.Files;

            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 404;
                return null;
            }


            Company newCompany = new Company(company.Name, company.Description, company.Number, company.Email);

            _db.Companys.Add(newCompany);
            await _db.SaveChangesAsync();

            await newCompany.SetImage(uploadedFile, _appEnvironment);

            _db.CompanyUsers.Add(new Models.Domain.ManyToMany.CompanyUser(userId, newCompany.Id));
            await _db.SaveChangesAsync();
            //return newCompany;
            return JsonConvert.SerializeObject(new { newCompany.Id, newCompany.Name }, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }


        // [Authorize]
        //null-нет доступа
        [HttpPost("ChangeCompany")]
        public async Task<bool?> ChangeCompany([FromForm]Company company, [FromForm] IFormFile[] uploadedFile = null)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }


            if (company.Id == 0)
                ModelState.AddModelError("Id", "Не передан Id");
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 404;
                return null;
            }

            var oldCompany = await Company.GetIfAccess(_db, userId, company.Id);

            //var oldCompany = await _db.Companys.FirstOrDefaultAsync(x1 => x1.Id == company.Id);
            if (oldCompany == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //byte[] newImage = null;
            await oldCompany.SetImage(uploadedFile, _appEnvironment);

            oldCompany.ChangeData(company.Name, company.Description, company.Number, company.Email);//, company.Image);
            await _db.SaveChangesAsync();
            return true;//oldCompany
        }

        [HttpPost("AddUserToCompany")]
        public async Task<bool?> AddUserToCompany([FromForm]int companyId, string newUserId)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }
            var company = await Company.GetIfAccess(_db, userId, companyId);
            if (company == null)
                return null;
            return await company.AddHeadUser(_db, newUserId);

        }

        [HttpPost("DeleteUserFromCompany")]
        public async Task<bool?> DeleteUserFromCompany([FromForm]int companyId, string newUserId)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }
            var company = await Company.GetIfAccess(_db, userId, companyId);
            if (company == null)
                return null;
            return await company.DeleteHeadUser(_db, newUserId);

        }



        // [Authorize]
        [HttpPost("CreateProject")]
        public async Task<int?> CreateProject([FromForm]Project project, [FromForm]string[] competences, [FromForm]string[] townNames, [FromForm]IFormFileCollection uploads = null)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }

            if (!ModelState.IsValid)
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
            await newProject.AddImagesToDbSystem(_db, _appEnvironment, uploads);
            await newProject.AddCompetences(_db, competences);

            await Project.AddTowns(_db, newProject.Id, townNames.ToList());

            // await _db.SaveChangesAsync();

            return newProject.Id;


        }

        // [Authorize]
        [HttpPost("ChangeProject")]
        public async Task<bool> ChangeProject([FromForm]Project project, [FromForm]IFormFileCollection uploads, [FromForm] int[] deleteImages, [FromForm]int[] competenceIds)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return false;
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 404;
                return false;
            }

            var oldProj = await ApplicationUser.CheckAccessEditProject(_db, project.Id, userId);
            if (oldProj == null)
            {
                Response.StatusCode = 404;
                return false;
            }
            oldProj.ChangeData(project.Name, project.Description, project.Payment);
            await _db.SaveChangesAsync();

            await oldProj.AddImagesToDbSystem(_db, _appEnvironment, uploads);
            await Project.DeleteImagesFromDb(_db, oldProj.Id, deleteImages);
            await oldProj.DeleteCompetences(_db, competenceIds);

            return true;

        }



        //меняет статус проекта на новый
        // [Authorize]
        [HttpPost("ChangeStatusProject")]
        public async Task<bool> ChangeStatusProject([FromForm]int projectId, [FromForm]StatusProject newStatus)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return false;
            }

            var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);
            if (proj == null)
            {
                Response.StatusCode = 404;
                return false;
            }
            await proj.SetStatus(_db, newStatus);
            return true;
        }


        // [Authorize]
        [HttpPost("ChangeStatusStudent")]
        public async Task<bool> ChangeStatusStudent([FromForm]int projectId, [FromForm]string studentId, [FromForm]StatusInProject newStatus)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return false;
            }

            var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);
            if (proj == null)
            {
                Response.StatusCode = 404;
                return false;
            }

            return await proj.ChangeStatusUser(_db, newStatus, studentId);
        }




        //[Authorize]
        [HttpGet("GetStudents")]
        public async Task GetStudents([FromForm]int projectId, [FromForm] StatusInProject status)
        {

            if (status != StatusInProject.Approved && status != StatusInProject.Canceled && status != StatusInProject.InProccessing)
                return;// null;

            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return;// null;
            }

            var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);

            if (proj == null)
            {
                Response.StatusCode = 404;
                return;// null;
            }

            var usersShort = await proj.GetStudentsShortEntity(_db, status);
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(usersShort, new JsonSerializerSettings { Formatting = Formatting.Indented }));
            //return usersShort;
        }

    }
}