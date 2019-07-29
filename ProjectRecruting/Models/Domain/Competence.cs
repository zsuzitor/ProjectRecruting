using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Competence
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public List<CompetenceProject> CompetenceProjects { get; set; }

        public Competence()
        {

        }

        public Competence(string name):this()
        {
            Name = name;
        }

        public async static Task<List<int>> SortByActual(ApplicationDbContext db, List<int> competenceIds)
        {
            return await db.CompetenceProjects.Where(x1 => competenceIds.Contains(x1.CompetenceId)).//Select(x1 => x1.ProjectId).
               GroupBy(x1 => x1.CompetenceId).OrderBy(x1 => x1.Count()).Select(x1 => x1.Key).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }
        public async static Task<List<Competence>> SortByActualEntity(ApplicationDbContext db, List<int> competenceIds)
        {
            return await db.CompetenceProjects.Where(x1 => competenceIds.Contains(x1.CompetenceId)).//Select(x1 => x1.ProjectId).
               GroupBy(x1 => x1.CompetenceId).Join(db.Competences, x1 => x1.Key, x2 => x2.Id, (x1, x2) => new { group = x1, entity = x2 }).
               OrderBy(x1 => x1.group.Count()).Select(x1 => x1.entity).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }

        //составляем запрос
        public static IQueryable<Competence> GetActualQueryEntityInTown(ApplicationDbContext db, int townId)
        {
            return db.CompetenceProjects.Join(db.ProjectTowns, x1 => x1.ProjectId, x2 => x2.ProjectId, (x1, x2) => new { competenceId = x1.CompetenceId, townId = x2.TownId }).
               Where(x1=>x1.townId==townId).
                GroupBy(x1 => x1.competenceId)
               .Join(db.Competences, x1 => x1.Key, x2 => x2.Id, (x1, x2) => new { group = x1, entity = x2 }).
               OrderBy(x1 => x1.group.Count()).Select(x1 => x1.entity);
        }
        //получаем полные данные
        public async static Task<List<Competence>> GetActualEntityInTown(ApplicationDbContext db, int townId)
        {
            return await Competence.GetActualQueryEntityInTown(db, townId).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }
        //получаем сокращенные данные
        public async static Task<List<CompetenceShort>> GetActualShortEntityInTown(ApplicationDbContext db, int townId)
        {
            return await Competence.GetActualQueryEntityInTown(db, townId).Select(x1 => new CompetenceShort(x1.Name, x1.Id)).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }

        public async static Task<List<int>> SortByActual(ApplicationDbContext db)
        {
            return await db.CompetenceProjects.GroupBy(x1 => x1.CompetenceId).OrderBy(x1 => x1.Count()).Select(x1 => x1.Key).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }

        public async static Task<List<int>> GetActualIds(ApplicationDbContext db)
        {
            return await db.CompetenceProjects.GroupBy(x1=>x1.CompetenceId).OrderBy(x1=> x1.Count()).Select(x1=>x1.Key).ToListAsync();
        }




        public async static Task<List<CompetenceShort>> GetShortsData(ApplicationDbContext db, List<int> competenceIds)
        {
            return await db.Competences.Where(x1 => competenceIds.Contains(x1.Id)).Select(x1 => new CompetenceShort(x1.Name, x1.Id)).ToListAsync();
        }
        public async static Task<List<CompetenceShort>> GetShortsData(ApplicationDbContext db)
        {
            return await db.Competences.Select(x1 => new CompetenceShort(x1.Name, x1.Id)).ToListAsync();
        }

        //просто получение, не обязательно актуальных
        public async static Task<List<int>> GetByTown(ApplicationDbContext db, int townId)
        {
            var projIds= await db.ProjectTowns.Where(x1 => x1.TownId == townId).Select(x1 => x1.ProjectId).Distinct().ToListAsync();
            return await db.CompetenceProjects.Where(x1 => projIds.Contains(x1.ProjectId)).Select(x1 => x1.Id).ToListAsync() ;
            
        }

        public async static Task<List<int>> GetActualInTown(ApplicationDbContext db, int townId)
        {
            var projIds = await db.ProjectTowns.Where(x1 => x1.TownId == townId).Select(x1 => x1.ProjectId).Distinct().ToListAsync();
            return await db.CompetenceProjects.Where(x1 => projIds.Contains(x1.ProjectId)).GroupBy(x1 => x1.CompetenceId).OrderBy(x1 => x1.Count()).Select(x1 => x1.Key).ToListAsync();

        }

        //public async static Task<List<int>> GetActualInTown(ApplicationDbContext db, int townId)
        //{

        //    return await Competence.SortByActual(db, await Competence.GetByTown(db, townId));
        //}
    }
}
