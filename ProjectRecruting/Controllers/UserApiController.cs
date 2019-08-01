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
        public async Task<bool?> ChangeStatusStudentInProject([FromForm]int projectId,[FromForm]Models.StatusInProject newStatus)
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

        //студент отзывает заявку
        //[HttpPost]
        //public async Task<bool?> CancelByStudent([FromForm]int projectId)
        //{
        //    string userId = AuthJWT.GetCurrentId(HttpContext, out int status);

        //    if (status != 0 || userId == null)
        //    {
        //        Response.StatusCode = 401;
        //        return null;
        //    }
        //    var project = await Project.Get(_db, projectId);
        //    return await project.CreateChangeStatusUser(_db, Models.StatusInProject.CanceledByStudent, userId);
        //}


        //просмотреть список актуальных проектов для города #TODO надо подсвечивать куда подал заявку
        [HttpGet("GetActualProject")]
        public async Task<List<ProjectShort>> GetActualProject([FromForm]string town)
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);
            if (status != 0 || userId == null)
            {
                Response.StatusCode = 401;
                return null;
            }
            List<ProjectShort> res = new List<ProjectShort>();

            if (town == null)
            {
                //выбрать все проекты независимо от города
                return await Project.GetShortsData(_db, await Project.SortByActual(_db));
            }
            string townLower = town.ToLower().Trim();
            var townDb = await Town.GetByName(_db, townLower);
            if (townDb == null)
                return res;

            return await Project.GetActualShortEntityInTown(_db, townDb.Id, userId);



        }
        [HttpGet]
        public async Task<Project> GetProject(int id)
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
            return  await Competence.GetActualShortEntityInTown(_db, townDb.Id);

            //#TODO сломает всю сортировку
            //return await Competence.GetShortsData(_db, actualListIds);

        }


    }
}