using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProjectRecruting.Data;
using ProjectRecruting.Models;
using ProjectRecruting.Models.Domain;

namespace ProjectRecruting.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        //подать заявку, отклонить заявку, просмотреть список актуальных проектов и список актуальных навыков для города или вообще

        readonly ApplicationDbContext _db = null;

        public UserApiController(ApplicationDbContext db)
        {
            _db = db;
        }


        /// <summary>
        /// изменение(добавление, если ее нет) статуса студента в проекте(для студента)
        /// </summary>
        /// <param name="projectId">id проекта</param>
        /// <param name="newStatus">статус проекта, enum-StatusInProject</param>
        /// <returns>true-существующая запись обновлена, false-добавлена новая, null-сейчас не обрабатывается-произошла ошибка</returns>
        ///  /// <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>
        /// <response code="404">проект не найден</response>
        /// <response code="400">переданы не валидные данные(статус))</response>
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [HttpPost("ChangeStatusStudentInProject")]
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
            var project = await Project.Get(_db, projectId);
            if (project == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return await project.CreateChangeStatusUser(_db, newStatus, userId);
        }



        /// <summary>
        /// просмотреть список актуальных проектов для города
        /// </summary>
        /// <param name="town">название города</param>
        /// <returns></returns>
        [HttpGet("GetActualProject")]
        public async Task GetActualProject([FromForm]string town)//<List<ProjectShort>>
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            //if (status != 0 || userId == null)
            //{
            //    Response.StatusCode = 401;
            //    return null;
            //}
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
            var res = await Project.GetActualShortEntityInTown(_db, townId, userId);
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
        [HttpGet("GetActualCompetences")]
        public async Task GetActualCompetences([FromForm]string town)
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
        /// <returns></returns>
        [HttpGet("GetProjectsCompany")]
        public async Task GetProjectsCompany([FromForm]int companyId, [FromForm]int? townId)
        {
            var res = await Company.GetProjectsByActual(_db, companyId, townId);
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(res, new JsonSerializerSettings { Formatting = Formatting.Indented }));

        }

        /// <summary>
        /// получить компании пользователя
        /// </summary>
        /// <returns></returns>
        /// <response code="401"> ошибка дешифрации токена, просрочен, изменен, не передан </response>

        [ProducesResponseType(401)]
        [HttpGet("GetUserCompanys")]
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
        [HttpGet("GetUserResponsibilityProjects")]
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
        [HttpGet("GetUserRequests")]
        public async Task GetUserRequests([FromForm] StatusInProject statusInProject)
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

    }
}