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
        public async Task<bool?> RequestStudent(int projectId)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var project = await Project.Get(_db, projectId);
            return await project.CreateChangeStatusUser(_db, Models.StatusInProject.InProccessing, userId);
        }

        //студент отзывает заявку
        public async Task<bool?> CancelByStudent(int projectId)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var project = await Project.Get(_db, projectId);
            return await project.CreateChangeStatusUser(_db, Models.StatusInProject.CanceledByStudent, userId);
        }


        //просмотреть список актуальных проектов для города #TODO надо подсвечивать куда подал заявку
        public async Task<List<ProjectShort>> GetActualProject(string town)
        {
            List<ProjectShort> res = new List<ProjectShort>();
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            if (town == null)
            {
                //выбрать все проекты независимо от города
                return await Project.GetShortsData(_db,await Project.SortByActual(_db));
            }
            string townLower = town.ToLower().Trim();
            var townDb = Town.GetByName(_db, townLower);
            if (townDb == null)
                return res;
            //var projids = await Project.GetByTown(_db, townDb.Id);
            //_db.ProjectUsers.Join(projids,x1=>x1.ProjectId,x2=>x2,(x1,x2)=>)
            //var actualListIds= await _db.ProjectUsers.Where(x1 => projids.Contains(x1.ProjectId)).Select(x1 => x1.ProjectId).
            //     GroupBy(x1=>x1).OrderBy(x1=>x1.Count()).Select(x1=>x1.Key).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
            //var actualListIds = await _db.ProjectUsers.Where(x1 => projids.Contains(x1.ProjectId)).//Select(x1 => x1.ProjectId).
            //    GroupBy(x1 => x1.ProjectId).OrderBy(x1 => x1.Count()).Select(x1 => x1.Key).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
            var actualListIds =await Project.GetActualInTown(_db, townDb.Id);

            //#TODO сломает всю сортировку
            return await Project.GetShortsData(_db, actualListIds);

        }


        //список актуальных навыков для города
        public async Task<List<CompetenceShort>> GetActualCompetences(string town)
        {
            List<CompetenceShort> res = new List<CompetenceShort>();
            //var claimsIdentity = this.User.Identity as ClaimsIdentity;
            //var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            if (town == null)
            {
                //выбрать все проекты независимо от города
                return await Competence.GetShortsData(_db, await Competence.SortByActual(_db));
            }
            string townLower = town.ToLower().Trim();
            var townDb = Town.GetByName(_db, townLower);
            if (townDb == null)
                return res;
            var actualListIds = await Competence.GetActualInTown(_db, townDb.Id);

            //#TODO сломает всю сортировку
            return await Competence.GetShortsData(_db, actualListIds);

        }


    }
}