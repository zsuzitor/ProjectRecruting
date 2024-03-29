﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectRecruting.Data;
using ProjectRecruting.Models;
using ProjectRecruting.Models.Domain;
using ProjectRecruting.Models.ShortModel;
using ProjectRecruting.Models.ResultModel;
using ProjectRecruting.Models.services;

namespace ProjectRecruting.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        //подать заявку, отклонить заявку, просмотреть список актуальных проектов и список актуальных навыков для города или вообще

        readonly ApplicationDbContext _db = null;
        readonly UserManager<ApplicationUser> _userManager = null;

        public UserApiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }


        /// <summary>
        /// изменение данных учетной записи пользователя
        /// </summary>
        /// <param name="newUser">новые данные пользователя</param>
        /// <param name="competences">названия компетенций для добавления</param>
        /// <param name="competenceIds">id компетенций для удаления</param>
        /// <returns></returns>
        ///  <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        [HttpPost("change-user-data")]
        public async Task<bool?> ChangeUserData([FromForm]ApplicationUser newUser, [FromForm]string[] competences, [FromForm]int[] competenceIds)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int statusId);
            if (statusId != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return false;
            }
            newUser.Id = userId;
            newUser.Validation(new ValidationInput());
            var user = await ApplicationUser.ChangeData(_db, _userManager, newUser);
            await user.AddCompetences(_db, competences);
            await user.DeleteCompetences(_db, competenceIds);

            return true;
        }




        /// <summary>
        /// изменение(добавление, если ее нет) статуса студента в проекте(для студента)
        /// </summary>
        /// <param name="projectId">id проекта</param>
        /// <param name="newStatus">статус проекта, enum-StatusInProject</param>
        /// <returns>true-существующая запись обновлена, false-добавлена новая, null-сейчас не обрабатывается-произошла ошибка</returns>
        ///  <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">проект не найден</response>
        /// <response code="400">переданы не валидные данные(статус))</response>
        /// <response code="406">почта не подтверждена</response>
        ///  <response code="527">параллельный запрос уже изменил данные</response>
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(406)]
        [ProducesResponseType(527)]
        [HttpPost("change-status-student-in-project")]
        public async Task<bool?> ChangeStatusStudentInProject([FromForm]int projectId, [FromForm]Models.StatusInProject newStatus)
        {
            if (newStatus != StatusInProject.InProccessing && newStatus != StatusInProject.CanceledByStudent)
            {
                Response.StatusCode = 400;
                return null;
            }
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }

            //---------------этот кусок нужен
            //var user = await ApplicationUser.Get(_userManager, userId);
            //bool mailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            //if (!mailConfirmed)
            //{
            //    Response.StatusCode = 406;
            //    return null;
            //}


            var project = await Project.Get(_db, projectId);
            if (project == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            bool? res = await project.CreateChangeStatusUser(_db, newStatus, userId);
            if (res == null)
            {
                Response.StatusCode = 527;
                return null;// null;
            }

            return res;
        }


        /// <summary>
        /// изменение(добавление, если ее нет) статуса пользователя в компании(для пользователя)
        /// </summary>
        /// <param name="companyId">id проекта</param>
        /// <param name="newStatus">статус проекта, enum-StatusInProject</param>
        /// <returns>true-существующая запись обновлена, false-добавлена новая, null-сейчас не обрабатывается-произошла ошибка</returns>
        ///  <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">компания не найдена</response>
        /// <response code="400">переданы не валидные данные(статус))</response>
        /// <response code="406">почта не подтверждена</response>
        ///  <response code="527">параллельный запрос уже изменил данные</response>
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(406)]
        [ProducesResponseType(527)]
        [HttpPost("change-status-student-in-project")]
        public async Task<bool?> ChangeStatusUserInCompany([FromForm]int companyId, [FromForm]Models.StatusInCompany newStatus)
        {
            if (newStatus != StatusInCompany.RequestedByUser && newStatus != StatusInCompany.Empty)
            {
                Response.StatusCode = 400;
                return null;
            }
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }

            //---------------этот кусок нужен
            //var user = await ApplicationUser.Get(_userManager, userId);
            //bool mailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            //if (!mailConfirmed)
            //{
            //    Response.StatusCode = 406;
            //    return null;
            //}


            var company = await Company.Get(_db, companyId);
            if (company == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            bool? res = await company.CreateChangeStatusUser(_db, newStatus, userId);
            if (res == null)
            {
                Response.StatusCode = 527;
                return null;// null;
            }

            return res;
        }


        /// <summary>
        /// просмотреть список актуальных проектов для города
        /// </summary>
        /// <param name="town">название города</param>
        /// <param name="lastId">id последней загруженной записи</param>
        /// <returns></returns>
        [HttpGet("get-actual-project")]
        public async Task GetActualProject(string town, int lastId=0)//<List<ProjectShort>>
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                userId = null;
            }
            //List<ProjectShort> res = new List<ProjectShort>();

            int? townId = null;
            if (town != null)
            {
                string townLower = town.ToLower().Trim();
                var townDb = await Town.GetByName(_db, townLower);
                if (townDb == null)
                    return;// new List<ProjectShort>();
                townId = townDb.Id;
            }
            var res = await Project.GetActualShortEntityWithStatus(_db, townId, userId,lastId);
            await ProjectShort.SetMainImages(_db, res);
            //return res;
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));


        }
        //[HttpGet("GetProject")]
        //public async Task<Project> GetProject([FromForm]int id)
        //{
        //    return await Project.Get(_db, id);
        //}



        /// <summary>
        /// список актуальных навыков для города
        /// </summary>
        /// <param name="town">название города</param>
        /// <returns></returns>
        [HttpGet("get-actual-competences")]
        public async Task GetActualCompetences(string town)
        {
            //List<CompetenceShort> res = new List<CompetenceShort>();

            int? townId = null;
            if (town != null)
            {
                string townLower = town.ToLower().Trim();
                var townDb = await Town.GetByName(_db, townLower);
                if (townDb == null)
                    return;// new List<CompetenceShort>();
                townId = townDb.Id;
            }

            var res = await Competence.GetActualShortEntityInTown(_db, townId);

            //return await Competence.GetActualShortEntityInTown(_db, townId);

            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));

        }

        /// <summary>
        /// получить проекты компании
        /// </summary>
        /// <param name="companyId">id компании</param>
        /// <param name="townId">id города</param>
        /// <param name="lastId">id последней загруженной записи</param>
        /// <returns></returns>
        [HttpGet("get-projects-company")]
        public async Task GetProjectsCompany(int companyId, int? townId, int lastId=0)
        {
            var res = await Company.GetProjectsByActual(_db, companyId, townId,lastId);
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));

        }


        /// <summary>
        /// получить список компаний
        /// </summary>
        /// <param name="townId">id города</param>
        /// <param name="lastId">id последней загруженной записи</param>
        /// <returns></returns>
        /// 
        [HttpGet("get-actual-companys")]
        public async Task GetActualCompanys(int? townId, int lastId = 0)
        {
            var res = await Company.GetActualEntity(_db, townId,lastId);


            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));

        }

        /// <summary>
        /// получить компании пользователя
        /// </summary>
        /// <returns></returns>
        /// <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>

        [ProducesResponseType(401)]
        [HttpGet("get-user-companys")]
        public async Task GetUserCompanys()
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return;// null;
            }
            var res = (await ApplicationUser.GetUserCompanys(_db, userId)).Select(x1 => new CompanyShort(x1));
            //return ;
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));

        }


        /// <summary>
        /// проекты за которые пользователь отвечает 
        /// </summary>
        /// <returns></returns>
        ///  <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        [ProducesResponseType(401)]
        [HttpGet("get-user-responsibility-projects")]
        public async Task GetUserResponsibilityProjects()
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return;// null;
            }
            var res = await ApplicationUser.GetUserResponsibilityProjects(_db, userId);
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));

        }

        /// <summary>
        /// получить список проектов пользователя(в которые у него есть заявки) по определенному статусу
        /// </summary>
        /// <param name="statusInProject">статус в проекте enum-StatusInProject</param>
        /// <returns></returns>
        ///  <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        [ProducesResponseType(401)]
        [HttpGet("get-user-requests")]
        public async Task GetUserRequests(StatusInProject statusInProject)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return;// null;
            }

            var res = await ApplicationUser.GetUserRequests(_db, userId, statusInProject);
            //res.ForEach(x1=>
            //{

            //});
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));
            //return await ApplicationUser.GetUserRequests(_db, userId,statusInProject);

        }

        /// <summary>
        /// получить список актуальных проектов название которых содержит  projectName
        /// </summary>
        /// <param name="projectName">строка вхождение которой ищем</param>
        /// <param name="townId">id города</param>
        /// <param name="lastId">id последней загруженной записи</param>
        /// <returns></returns>
        [HttpGet("get-project-by-contains-name")]
        public async Task GetProjectByContainsName(string projectName, int? townId, int lastId = 0)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                userId = null;
            }
            var res = await Project.GetByStartName(_db, townId, userId, projectName, lastId);
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }



        /// <summary>
        /// получение проекта
        /// </summary>
        /// <param name="id">id проекта</param>
        /// <returns></returns>
        /// <response code="404"> проект не найден</response>
        [ProducesResponseType(404)]
        [HttpGet("get-project")]
        public async Task GetProject(int? id)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                userId = null;
            }
            var proj = await Project.Get(_db, id);
            if (proj == null)
            {
                Response.StatusCode = 404;
                return;// null;
            }
            var res = await ProjectPage.LoadAllForView(_db, proj, userId);


            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }


        /// <summary>
        /// получить компанию
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="404"> компания не найдена</response>
        [ProducesResponseType(404)]
        [HttpGet("get-company")]
        public async Task GetCompany(int id)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                userId = null;
            }
            var company = await Company.Get(_db, id);//(int)
            CompanyPage res = await CompanyPage.LoadAllForView(_db, company, userId);

            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

    }
}