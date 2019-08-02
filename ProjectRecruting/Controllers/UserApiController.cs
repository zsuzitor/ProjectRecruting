using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models;
using ProjectRecruting.Models.Domain;

namespace ProjectRecruting.Controllers
{
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

        //true-существующая запись обновлена, false-добавлена новая, null-сейчас не обрабатывается-произошла ошибка
        //оставить\обновить заявку пользователя на проект
        [HttpPost("ChangeStatusStudentInProject")]
        public async Task<bool?> ChangeStatusStudentInProject([FromForm]int projectId, [FromForm]Models.StatusInProject newStatus)
        {
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


        //просмотреть список актуальных проектов для города #TODO надо подсвечивать куда подал заявку
        [HttpGet("GetActualProject")]
        public async Task<List<ProjectShort>> GetActualProject([FromForm]string town)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            //if (status != 0 || userId == null)
            //{
            //    Response.StatusCode = 401;
            //    return null;
            //}
            List<ProjectShort> res = new List<ProjectShort>();

            int? townId = null;
            if (town != null)
            {
                string townLower = town.ToLower().Trim();
                var townDb = await Town.GetByName(_db, townLower);
                if (townDb == null)
                    return res;
                townId = townDb.Id;
            }
            
            return await Project.GetActualShortEntityInTown(_db, townId, userId);



        }
        [HttpGet("GetProject")]
        public async Task<Project> GetProject([FromForm]int id)
        {
            return await Project.Get(_db, id);
        }


        //список актуальных навыков для города
        [HttpGet("GetActualCompetences")]
        public async Task<List<CompetenceShort>> GetActualCompetences([FromForm]string town)
        {
            List<CompetenceShort> res = new List<CompetenceShort>();

            if (town == null)
            {
                //выбрать все проекты независимо от города
                return await Competence.GetShortsData(_db, await Competence.GetActualIds(_db));
            }
            string townLower = town.ToLower().Trim();
            var townDb = await Town.GetByName(_db, townLower);
            if (townDb == null)
                return res;
            return await Competence.GetActualShortEntityInTown(_db, townDb.Id);


        }


        [HttpGet("GetProjectsCompany")]
        public async Task<List<Project>> GetProjectsCompany([FromForm]int companyId, [FromForm]int?townId)
        {
            return await Company.GetProjectsByActual(_db,companyId,townId);

        }


        [HttpGet("GetUserCompanys")]
        public async Task<List<Company>> GetUserCompanys()
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }
            return await ApplicationUser.GetUserCompanys(_db, userId);

        }

        //проекты за которые пользователь отвечает
        [HttpGet("GetUserResponsibilityProjects")]
        public async Task<List<Company>> GetUserResponsibilityProjects()
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }
            return await ApplicationUser.GetUserCompanys(_db, userId);

        }


        [HttpGet("GetUserRequests")]
        public async Task<List<Project>> GetUserRequests([FromForm] StatusInProject statusInProject)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }
           
            return await ApplicationUser.GetUserRequests(_db, userId,statusInProject);

        }

    }
}