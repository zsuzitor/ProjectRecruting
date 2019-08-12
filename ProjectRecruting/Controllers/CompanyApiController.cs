using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProjectRecruting.Data;
using ProjectRecruting.Models;
using ProjectRecruting.Models.Domain;
using ProjectRecruting.Models.services;

namespace ProjectRecruting.Controllers
{
    [Produces("application/json")]
    [Route("api/company")]
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

        /// <summary>
        /// создание компании
        /// </summary>
        /// <param name="company">поля компании</param>
        /// <param name="uploadedFile">аватар компании</param>
        /// <returns>{ Id, Name }</returns>
        /// <response code="401">ошибка дешифрации токена, просрочен, изменен, не передан</response>
        /// <response code="400">переданы не валидные данные</response>
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [HttpPost("create-company")]
        public async Task CreateCompany([FromForm]Company company, [FromForm]IFormFile[] uploadedFile = null)
        {
            //var file = HttpContext.Request.Form.Files;

            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return;
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return;
            }
            Company newCompany = new Company(company.Name, company.Description, company.Number, company.Email);

            _db.Companys.Add(newCompany);
            await _db.SaveChangesAsync();

            await newCompany.SetImage(_db, uploadedFile, _appEnvironment);

            _db.CompanyUsers.Add(new Models.Domain.ManyToMany.CompanyUser(userId, newCompany.Id, StatusInCompany.Moderator));
            await _db.SaveChangesAsync();
            //return newCompany;
            Response.ContentType = "application/json";

            await Response.WriteAsync(JsonConvert.SerializeObject(new { newCompany.Id, newCompany.Name }, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }


        /// <summary>
        /// изменение компании
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="company">данные компании</param>
        /// <param name="uploadedFile">новый аватар компании,если null то остается старый</param>
        /// <returns>bool?--true-добавлена, null-что то не так</returns>
        /// <response code="400">плохие данные</response>
        /// <response code="401">ошибка дешифрации токена, просрочен, изменен, не передан</response>
        /// <response code="404">компания для изменения не найдена</response>
        /// <response code="527">параллельный запрос уже изменил данные</response>
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(527)]
        [HttpPost("change-company")]
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
                Response.StatusCode = 400;
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

            try
            {
                oldCompany.ChangeData(company.Name, company.Description, company.Number, company.Email);//, company.Image);
                await _db.SaveChangesAsync();
                await oldCompany.SetImage(_db, uploadedFile, _appEnvironment);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Response.StatusCode = 527;
                return null;
            }

            return true;//oldCompany
        }


        /// <summary>
        /// добавление управляющего в компанию(должна быть подана заявка)
        /// </summary>
        /// <param name="companyId">id компании в которую добавляес пользователя</param>
        /// <param name="newUserId">id пользователя которого добавляем</param>
        /// <returns></returns>
        /// <response code="401">. ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">компания не найдена</response>
        /// <response code="527">параллельный запрос уже изменил данные</response>
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(527)]
        [HttpPost("add-user-to-company")]
        public async Task<bool?> AddUserToCompany([FromForm]int companyId, [FromForm] string newUserId, [FromForm] StatusInCompany newStatus)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }
            var company = await Company.GetIfAccess(_db, userId, companyId);
            if (company == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            var res = await company.AddHeadUser(_db, newUserId);
            if (res == null)
            {
                Response.StatusCode = 527;
                return null;
            }

            return res;

        }


        /// <summary>
        ///  удалить управляющего из компании
        /// </summary>
        /// <param name="companyId">id компании из которой удаляем</param>
        /// <param name="newUserId">id пользователя которого удаляем</param>
        /// <returns></returns>
        ///  <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">компания не найдена</response>
        /// <response code="527">параллельный запрос уже изменил данные</response>
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(527)]
        [HttpPost("delete-user-from-company")]
        public async Task<bool?> DeleteUserFromCompany([FromForm]int companyId, [FromForm] string newUserId)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }
            var company = await Company.GetIfAccess(_db, userId, companyId);
            if (company == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            var res = await company.RemoveUser(_db, newUserId, StatusInCompany.Moderator);
            if (res == null)
            {
                Response.StatusCode = 527;
                return null;
            }
            return res;

        }


        /// <summary>
        /// создание проекта
        /// </summary>
        /// <param name="project">данные проекта</param>
        /// <param name="competences">список компетенций</param>
        /// <param name="townNames">список названий городов</param>
        /// <param name="uploadedFile">изображения</param>
        /// <returns></returns>
        /// <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">компания не найдена</response>
        /// <response code="400">переданы не валидные данные</response>
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [HttpPost("create-project")]
        public async Task<int?> CreateProject([FromForm]Project project, [FromForm]string[] competences = null,
            [FromForm]string[] townNames = null, [FromForm] IFormFile[] uploadedFile = null)
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

            if (townNames == null || townNames.Length == 0)
            {
                Response.StatusCode = 400;
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
            await newProject.AddImagesToDbSystem(_db, _appEnvironment, uploadedFile);
            await newProject.AddCompetences(_db, competences);

            await Project.AddTowns(_db, newProject.Id, townNames.ToList());

            // await _db.SaveChangesAsync();

            return newProject.Id;


        }


        /// <summary>
        /// изменение проекта
        /// </summary>
        /// <param name="project">новые данные проекта</param>
        /// <param name="uploadedFile">изображения</param>
        /// <param name="deleteImages">ids изображений для удаления</param>
        /// <param name="competences">список названий компетенций</param>
        /// <param name="competenceIds">список id компетенций для удаления</param>
        /// <returns></returns>
        /// <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">проект не найден</response>
        /// <response code="400">переданы не валидные данные</response>
        /// <response code="527">параллельный запрос уже изменил данные</response>
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(527)]
        [HttpPost("change-project")]
        public async Task<bool> ChangeProject([FromForm]Project project, [FromForm] int[] deleteImages,
            [FromForm]string[] competences, [FromForm]int[] competenceIds, [FromForm]IFormFile[] uploadedFile = null)
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

            using (var tranzaction = _db.Database.BeginTransaction())
            {
                try
                {

                    oldProj.ChangeData(project.Name, project.Description, project.Payment);
                    await _db.SaveChangesAsync();
                    await oldProj.AddCompetences(_db, competences);
                    await oldProj.DeleteCompetences(_db, competenceIds);
                    await Project.DeleteImagesFromDb(_db, oldProj.Id, deleteImages);
                    tranzaction.Commit();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    tranzaction.Rollback();
                    Response.StatusCode = 527;
                    return false;
                }
                catch
                {
                    tranzaction.Rollback();
                    return false;
                }
            }

            await oldProj.AddImagesToDbSystem(_db, _appEnvironment, uploadedFile);

            return true;

        }



        /// <summary>
        /// изменение статуса проекта
        /// </summary>
        /// <param name="projectId">id проекта</param>
        /// <param name="newStatus">новый статус проекта</param>
        /// <returns></returns>
        /// <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">проект не найден</response>
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [HttpPost("change-status-project")]
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
            try
            {
                await proj.SetStatus(_db, newStatus);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Response.StatusCode = 527;
                return false;
            }

            return true;
        }

        /// <summary>
        /// изменение статуса студента в проекте(для управляющего проектом)
        /// </summary>
        /// <param name="projectId">id проекта</param>
        /// <param name="studentId">id студента</param>
        /// <param name="newStatus">новый статус студента enum-StatusInProject</param>
        /// <returns></returns>
        /// <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">проект не найден</response>
        /// <response code="527">параллельный запрос уже изменил данные</response>
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(527)]
        [HttpPost("change-status-student")]
        public async Task<bool> ChangeStatusStudent([FromForm]int projectId, [FromForm]string studentId, [FromForm]StatusInProject newStatus)
        {
            if (newStatus != StatusInProject.InProccessing && newStatus != StatusInProject.Canceled && newStatus != StatusInProject.Approved)
            {
                Response.StatusCode = 400;
                return false;
            }
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
            bool? res = await proj.ChangeStatusUserByLead(_db, newStatus, studentId);
            if (res == null)
            {
                Response.StatusCode = 527;
                return false;// null;
            }

            return (bool)res;
        }




        /// <summary>
        /// получить студентов проекта с определенным статусом
        /// </summary>
        /// <param name="projectId">id проекта</param>
        /// <param name="status">статус студентаов для выборки enum-StatusInProject</param>
        /// <returns></returns>
        /// <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">проект не найден</response>
        /// <response code="400">переданы не валидные данные(статус))</response>
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [HttpGet("get-students")]
        public async Task GetStudents(int projectId, StatusInProject status)
        {

            if (status != StatusInProject.Approved && status != StatusInProject.Canceled && status != StatusInProject.InProccessing)
            {
                Response.StatusCode = 400;
                return;// null;
            }

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