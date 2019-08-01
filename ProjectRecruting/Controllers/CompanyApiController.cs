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
        readonly UserManager<ApplicationUser> _userManager = null;
        IHostingEnvironment _appEnvironment { get; set; }
        public CompanyApiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IHostingEnvironment appEnvironment)
        {
            _db = db;
            _userManager = userManager;
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
            //if (string.IsNullOrWhiteSpace(userId))
            //{
            //    Response.StatusCode = 404;
            //    return null;
            //}

            Company newCompany = new Company(company.Name, company.Description, company.Number, company.Email);


            //using (var binaryReader = new BinaryReader(uploadedFile[0].OpenReadStream()))
            //{
            //    newCompany.Image = binaryReader.ReadBytes((int)uploadedFile[0].Length);
            //}

            _db.Companys.Add(newCompany);
            await _db.SaveChangesAsync();
            //if (uploadedFile != null && uploadedFile.Length > 0)
            //{
            //    string path = "/images/uploads/company_" + newCompany.Id + "_mainimage";// + uploadedFile[0].FileName;
            //    // сохраняем файл в папку Files в каталоге wwwroot
            //    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
            //    {
            //        await uploadedFile[0].CopyToAsync(fileStream);
            //    }

            //}
            await newCompany.SetImage(uploadedFile, _appEnvironment);

            _db.CompanyUsers.Add(new Models.Domain.ManyToMany.CompanyUser(userId, newCompany.Id));
            await _db.SaveChangesAsync();
            //return newCompany;
            return JsonConvert.SerializeObject(new { newCompany.Id, newCompany.Name }, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }


        // [Authorize]
        [HttpPost]
        public async Task<Company> ChangeCompany([FromForm]Company company, [FromForm] IFormFile[] uploadedFile = null)
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
            //if (string.IsNullOrWhiteSpace(userId))
            //{
            //    Response.StatusCode = 404;
            //    return null;
            //}
            //var companyUser = await _db.CompanyUsers.FirstOrDefaultAsync(x1 => x1.CompanyId == company.Id&&x1.UserId==userId);
            //if (companyUser == null)
            //{
            //    Response.StatusCode = 404;
            //    return null;
            //}
            var oldCompany = await Company.GetIfAccess(_db, userId, company.Id);

            //var oldCompany = await _db.Companys.FirstOrDefaultAsync(x1 => x1.Id == company.Id);
            if (oldCompany == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //byte[] newImage = null;
            await oldCompany.SetImage(uploadedFile, _appEnvironment);
            //if (uploadedFile != null && uploadedFile.Length > 0)
            //{
            //    string path = "/images/uploads/company_" + oldCompany.Id + "_mainimage";// + uploadedFile[0].FileName;
            //    // сохраняем файл в папку Files в каталоге wwwroot
            //    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
            //    {
            //        await uploadedFile[0].CopyToAsync(fileStream);
            //    }
            //    //using (var binaryReader = new BinaryReader(uploadedFile[0].OpenReadStream()))
            //    //{
            //    //    newImage = binaryReader.ReadBytes((int)uploadedFile[0].Length);
            //    //}
            //}
            oldCompany.ChangeData(company.Name, company.Description, company.Number, company.Email);//, company.Image);
            await _db.SaveChangesAsync();
            return oldCompany;
        }


        // [Authorize]
        [HttpPost("CreateProject")]
        public async Task<Project> CreateProject([FromForm]Project project, [FromForm]string[] competences, [FromForm]string[] townNames, [FromForm]IFormFileCollection uploads = null)
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
            //if (string.IsNullOrWhiteSpace(userId))
            //{
            //    Response.StatusCode = 404;
            //    return null;
            //}
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

            var listTown = await Town.GetOrCreate(_db, townNames);
            foreach (var i in listTown)
            {
                _db.ProjectTowns.Add(new Models.Domain.ManyToMany.ProjectTown(i.Id, newProject.Id));
            }
            //var town=await Town.GetByName(_db,townName);
            //if(town==null)
            //{
            //    town = new Town(townName);
            //    _db.Towns.Add(town);
            //    await _db.SaveChangesAsync();
            //}
            //_db.ProjectTowns.Add(new Models.Domain.ManyToMany.ProjectTown(town.Id,newProject.Id));
            await _db.SaveChangesAsync();

            return newProject;


        }

        // [Authorize]
        [HttpPost]
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
            //if (string.IsNullOrWhiteSpace(userId))
            //{
            //    Response.StatusCode = 404;
            //    return false;
            //}
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

        //меняет статус проекта на закрытый
        ////[Authorize]
        //[HttpPost]
        //public async Task<bool> CloseProject([FromForm]int projectId)//#TODO хз стоит ли объединять с CompliteProject в метод ChangeStatusProject
        //{
        //    string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
        //    if (statusId != 0 || userId == null)
        //    {
        //        Response.StatusCode = 401;
        //        return false;
        //    }

        //    var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);
        //    if (proj == null)
        //    {
        //        Response.StatusCode = 404;
        //        return false;
        //    }
        //    await proj.SetStatus(_db, StatusProject.Closed);
        //    return true;
        //}


        //меняет статус проекта на выполненный
        // [Authorize]
        [HttpPost]
        public async Task<bool> ChangeStatusProject([FromForm]int projectId, [FromForm]StatusProject newStatus)//#TODO хз стоит ли объединять с CloseProject в метод ChangeStatusProject
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
        [HttpPost]
        public async Task<bool> ApproveStudent([FromForm]int projectId, [FromForm]string studentId)
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

            return await proj.ChangeStatusUser(_db, StatusInProject.Approved, studentId);
        }

        // [Authorize]
        [HttpPost]
        public async Task<bool> CancelStudent([FromForm]int projectId, [FromForm]string studentId)
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

            return await proj.ChangeStatusUser(_db, StatusInProject.Canceled, studentId);
        }


        //[Authorize]
        public async Task<List<UserShort>> GetStudents([FromForm]int projectId, [FromForm] StatusInProject status)
        {

            if (status != StatusInProject.Approved && status != StatusInProject.Canceled && status != StatusInProject.InProccessing)
                return null;

            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }

            var proj = await ApplicationUser.CheckAccessEditProject(_db, projectId, userId);

            if (proj == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var usersShort = await proj.GetStudentsShortEntity(_db, status);
            return usersShort;
        }

    }
}