using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain.ManyToMany;
using ProjectRecruting.Models.ShortModel;
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
        public List<CompetenceUser> CompetenceUsers { get; set; }

        public Competence()
        {
            CompetenceProjects = new List<CompetenceProject>();
            CompetenceUsers = new List<CompetenceUser>();
        }

        public Competence(string name) : this()
        {
            Name = name;
        }

        public void Validation(IInputValidator validator)
        {
            this.Name = validator.ValidateString(Name);
        }
        public static void Validation(IInputValidator validator,string[]mass)
        {
            validator.ValidateStringArray(mass);
        }

        public async static Task<List<Competence>> CreateInDbIfNeed(ApplicationDbContext db, string[] competences)
        {
            //List<Competence> res = new List<Competence>();
            if (competences == null || competences.Length == 0)
                return new List<Competence>();
            var competencesList = competences.Select(x1 => x1.ToLower().Trim()).ToList();
            var existsCompetences = await db.Competences.Where(x1 => competencesList.Contains(x1.Name)).ToListAsync();
            //db.CompetenceProjects.Where(x1=> existsCompetences.Contains(x1));


            existsCompetences.ForEach((x) =>
            {
                if (competencesList.Contains(x.Name))
                    competencesList.Remove(x.Name);
            });
            //competencesList.Remove(existsCompetences.Where(x1=> competencesList.Contains(x1.Name)));
            List<Competence> needAdded = competencesList.Select(x1 => new Competence(x1)).ToList();
            db.Competences.AddRange(needAdded);
            await db.SaveChangesAsync();


            needAdded.AddRange(existsCompetences);
            return needAdded;
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
        private static IQueryable<Competence> GetActualQueryEntityInTown(ApplicationDbContext db, int? townId)
        {
            return db.ProjectTowns.Where(x1 => townId==null?true: x1.TownId == townId).Join(db.CompetenceProjects, x1 => x1.ProjectId, x2 => x2.ProjectId, (x1, x2) => x2.CompetenceId).
                 GroupBy(x1 => x1)
               .Join(db.Competences, x1 => x1.Key, x2 => x2.Id, (x1, x2) => new { group = x1, entity = x2 }).
               OrderByDescending(x1 => x1.group.Count()).Select(x1 => x1.entity);

        }


        //получаем полные данные
        public async static Task<List<Competence>> GetActualEntityInTown(ApplicationDbContext db, int townId)
        {
            return await Competence.GetActualQueryEntityInTown(db, townId).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }
        //получаем сокращенные данные
        public async static Task<List<CompetenceShort>> GetActualShortEntityInTown(ApplicationDbContext db, int? townId)
        {
            return await Competence.GetActualQueryEntityInTown(db, townId).Select(x1 => new CompetenceShort(x1.Name, x1.Id)).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }

        public async static Task<List<int>> GetActualIds(ApplicationDbContext db)
        {
            return await db.CompetenceProjects.GroupBy(x1 => x1.CompetenceId).OrderBy(x1 => x1.Count()).Select(x1 => x1.Key).ToListAsync();
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
            return await db.ProjectTowns.Where(x1 => x1.TownId == townId).Select(x1 => x1.ProjectId).Distinct().
                Join(db.CompetenceProjects, x1 => x1, x2 => x2.ProjectId, (x1, x2) => x2.CompetenceId).ToListAsync();

        }

        public async static Task<List<int>> GetActualInTown(ApplicationDbContext db, int townId)
        {
            return await Competence.GetActualQueryEntityInTown(db, townId).Select(x1 => x1.Id).ToListAsync();


        }

    }
}
