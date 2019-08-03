using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Project
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Не указано имя")]
        public string Name { get; set; }
        public string Description { get; set; }

        public bool? Payment { get; set; }

        //заявки можнпо подавать только если проект не начат или начат но не закончен
        public StatusProject Status { get; set; }

        [Required(ErrorMessage = "Не указан id компании")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }


        public List<CompetenceProject> CompetenceProjects { get; set; }
        public List<ProjectUser> ProjectUsers { get; set; }

        public List<ProjectTown> ProjectTowns { get; set; }

        public List<Image> Images { get; set; }

        public Project()
        {
            Status = StatusProject.NotStarted;
            Payment = null;
        }

        public Project(string name, string description, bool? payment, int companyId) : this()
        {
            Name = name;
            Description = description;
            Payment = payment;
            CompanyId = companyId;
        }


        //НЕ загрузит их в сущность, только добавит в бд
        public async Task<List<Image>> AddImagesToDbSystem(ApplicationDbContext db, IHostingEnvironment appEnvironment, IFormFileCollection images)
        {
            //Tuple<>
            List<Image> res = new List<Image>();
            //var imgs = Image.GetBytes(images);
            if (images != null && images.Count > 0)
            {

                foreach (var i in images)
                    res.Add(new Image() { ProjectId = this.Id });
                db.Images.AddRange(res);
                await db.SaveChangesAsync();

                for (var i = 0; i < images.Count; ++i)
                {
                    string path = "/images/uploads/project_" + this.Id + "_" + res[i].Id;// + uploadedFile[0].FileName;
                                                                                         // сохраняем файл в папку Files в каталоге wwwroot
                    using (var fileStream = new FileStream(appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await images[i].CopyToAsync(fileStream);
                    }
                }

            }

            return res;
        }

        //просто удаление, есть ли доступ у пользователя не проверяется
        public async static Task<List<Image>> DeleteImagesFromDb(ApplicationDbContext db, int projectId, int[] imageIds)
        {
            if (imageIds == null || imageIds.Length == 0)
                return new List<Image>();
            var images = await db.Images.Where(x1 => imageIds.Contains(x1.Id) && x1.ProjectId == projectId).ToListAsync();
            db.Images.RemoveRange(images);
            await db.SaveChangesAsync();
            return images;
        }


        public void ChangeData(string name, string description, bool? payment)
        {
            Name = name;
            Description = description;
            Payment = payment;

        }


        public async Task SetStatus(ApplicationDbContext db, StatusProject newStatus)
        {
            this.Status = newStatus;
            await db.SaveChangesAsync();

        }


        public async static Task<Project> Get(ApplicationDbContext db, int id)
        {
            return await db.Projects.FirstOrDefaultAsync(x1 => x1.Id == id);
        }

        //не создает запись если ее нет
        public async Task<bool> ChangeStatusUser(ApplicationDbContext db, StatusInProject newStatus, string userId)
        {
            var record = await db.Entry(this).Collection(x1 => x1.ProjectUsers).Query().FirstOrDefaultAsync(x1 => x1.UserId == userId);
            if (record == null)
                return false;
            record.Status = newStatus;
            await db.SaveChangesAsync();
            return true;
        }


        //создает если надо и изменяет запись .(если записи нет то создаст ее.) у записи будет переданный статус
        public async Task<bool> CreateChangeStatusUser(ApplicationDbContext db, StatusInProject newStatus, string userId)
        {
            bool exists = false;
            var record = await db.Entry(this).Collection(x1 => x1.ProjectUsers).Query().FirstOrDefaultAsync(x1 => x1.UserId == userId);
            if (record == null)
                db.ProjectUsers.Add(new ProjectUser(userId, this.Id, newStatus));
            else
            {
                exists = true;
                record.Status = newStatus;
            }

            await db.SaveChangesAsync();
            return exists;
        }


        //добавление без валидации
        public async Task<List<Competence>> AddCompetences(ApplicationDbContext db, string[] competences)
        {
            //List<Competence> res = new List<Competence>();
            if (competences == null || competences.Length == 0)
                return new List<Competence>();
            var competencesList = competences.Select(x1 => x1.ToLower().Trim()).ToList();
            var existsCompetences = await db.Competences.Where(x1 => competences.Contains(x1.Name)).ToListAsync();
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

            List<CompetenceProject> forAddedRelation = new List<CompetenceProject>();
            needAdded.ForEach(x =>
            {
                forAddedRelation.Add(new CompetenceProject(x.Id, this.Id));
            });
            db.CompetenceProjects.AddRange(forAddedRelation);

            await db.SaveChangesAsync();
            return needAdded;
        }

        //удаление компетенций без валидации
        public async Task DeleteCompetences(ApplicationDbContext db, int[] competenceIds)
        {
            db.CompetenceProjects.RemoveRange(db.CompetenceProjects.Where(x1 => competenceIds.Contains(x1.Id) && x1.ProjectId == this.Id));

            await db.SaveChangesAsync();
        }

        public IQueryable<string> GetStudentsQuery(ApplicationDbContext db, StatusInProject status)
        {
            //List<UserShort> res = new List<UserShort>();
            return db.Entry(this).Collection(x1 => x1.ProjectUsers).Query().Where(x1 => x1.Status == status).Select(x1 => x1.UserId);

        }

        //список id пользователей проекта с определенным статусом
        public async Task<List<string>> GetStudents(ApplicationDbContext db, StatusInProject status)
        {
            return await this.GetStudentsQuery(db, status).ToListAsync();
            //List<UserShort> res = new List<UserShort>();
            // return await db.Entry(this).Collection(x1 => x1.ProjectUsers).Query().Where(x1 => x1.Status == status).Select(x1 => x1.UserId).ToListAsync();

        }

        //список  пользователей проекта с определенным статусом
        public async Task<List<UserShort>> GetStudentsShortEntity(ApplicationDbContext db, StatusInProject status)
        {
            //List<UserShort> res = new List<UserShort>();
            return await this.GetStudentsQuery(db, status).Join(db.Users, x1 => x1, x2 => x2.Id, (x1, x2) => new UserShort(x2.Name, x2.Id)).ToListAsync();

        }


        //без сортировки, все записи для переданного списка
        public async static Task<List<ProjectShort>> GetShortsData(ApplicationDbContext db, List<int> projectIds)
        {
            var res = await db.Projects.Where(x1 => projectIds.Contains(x1.Id)).Select(x1 => new ProjectShort(x1.Name, x1.Id)).ToListAsync();
            await ProjectShort.SetMainImages(db,res);
            return res;
        }

        //без осртировки, все записи
        public async static Task<List<ProjectShort>> GetShortsData(ApplicationDbContext db)
        {
            var res = await db.Projects.Select(x1 => new ProjectShort(x1.Name, x1.Id)).ToListAsync();
            await ProjectShort.SetMainImages(db, res);
            return res;
        }

        //без сортировки, просто все записи в городе
        public async static Task<List<int>> GetByTown(ApplicationDbContext db, int townId)
        {
            return await db.ProjectTowns.Where(x1 => x1.TownId == townId).Select(x1 => x1.ProjectId).Distinct().ToListAsync();
        }

        //сортирует список id по актуальности
        public async static Task<List<int>> SortByActual(ApplicationDbContext db, List<int> projectIds)
        {
            return await db.ProjectUsers.Where(x1 => projectIds.Contains(x1.ProjectId)).//Select(x1 => x1.ProjectId).
               GroupBy(x1 => x1.ProjectId).OrderBy(x1 => x1.Count()).Select(x1 => x1.Key).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }


        public async static Task<List<Project>> SortByActualEntity(ApplicationDbContext db, List<int> projectIds)
        {
            return await db.ProjectUsers.Where(x1 => projectIds.Contains(x1.ProjectId)).//Select(x1 => x1.ProjectId).
               GroupBy(x1 => x1.ProjectId).Join(db.Projects, x1 => x1.Key, x2 => x2.Id, (x1, x2) => new { group = x1, entity = x2 }).
               OrderBy(x1 => x1.group.Count()).Select(x1 => x1.entity).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }


        //составляем запрос
        public static IQueryable<Project> GetActualQueryEntityInTown(ApplicationDbContext db, int? townId)//#TODO можно вынести db.Projects в параметры а этот метод сделать оболочкой, если нужно будет
        {
            return db.ProjectTowns.Where(x1 => townId == null ? true : (x1.TownId == townId)).
                GroupJoin(db.ProjectUsers, x1 => x1.ProjectId, x2 => x2.ProjectId, (x, y) => new { proj = x, prUser = y }).
                SelectMany(x => x.prUser.DefaultIfEmpty(), (x, y) => x.proj.ProjectId).GroupBy(x1 => x1).Select(x1 => new { projId = x1.Key, count = x1.Count() }).
                Join(db.Projects, x1 => x1.projId, x2 => x2.Id, (x1, x2) => new { proj = x2, count = x1.count }).
                OrderByDescending(x1 => x1.count).Select(x1 => x1.proj);

        }
        //получаем полные данные
        public async static Task<List<Project>> GetActualEntityInTown(ApplicationDbContext db, int? townId)
        {
            return await Project.GetActualQueryEntityInTown(db, townId).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }
        //получаем сокращенные данные
        public async static Task<List<ProjectShort>> GetActualShortEntityInTown(ApplicationDbContext db, int? townId, string userId)
        {
            var projs = await Project.GetActualQueryEntityInTown(db, townId).Select(x1 => new ProjectShort(x1.Name, x1.Id)).ToListAsync();
            await ProjectShort.SetMainImages(db, projs);
            if (string.IsNullOrWhiteSpace(userId))
                return projs;
            var status = await db.ProjectUsers.Where(x1 => x1.UserId == userId && projs.FirstOrDefault(x2 => x2.ProjectId == x1.ProjectId) != null).ToListAsync();
            status.ForEach(x1 =>
            {
                var tmp = projs.FirstOrDefault(x2 => x2.ProjectId == x1.ProjectId);
                if (tmp != null)
                    tmp.Status = x1.Status;
            });
            return projs;
        }

        public async static Task<List<int>> SortByActual(ApplicationDbContext db)
        {
            return await db.ProjectUsers.GroupBy(x1 => x1.ProjectId).OrderBy(x1 => x1.Count()).Select(x1 => x1.Key).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }


        public async static Task<List<int>> GetActualInTown(ApplicationDbContext db, int townId)
        {
            return await Project.SortByActual(db, await Project.GetByTown(db, townId));
        }


        public async static Task<List<Town>> AddTowns(ApplicationDbContext db, int projectId, List<string> townNames)
        {
            var listTown = await Town.GetOrCreate(db, townNames);
            var townsId = listTown.Select(x1 => x1.Id);
            var dontAdd = await db.ProjectTowns.Where(x1 => x1.ProjectId == projectId && townsId.Contains(x1.TownId)).ToListAsync();
            var forAdd = listTown.Where(x1 => dontAdd.FirstOrDefault(x2 => x2.Id == x1.Id) == null).ToList();
            // listTown.RemoveRange();
            foreach (var i in forAdd)
            {
                db.ProjectTowns.Add(new Models.Domain.ManyToMany.ProjectTown(i.Id, projectId));
            }

            await db.SaveChangesAsync();
            return forAdd;
        }



    }
}
