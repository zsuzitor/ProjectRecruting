using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain.ManyToMany;
using ProjectRecruting.Models.ShortModel;
using ProjectRecruting.Models.ResultModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
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


        [Timestamp]
        public byte[] RowVersion { get; set; }

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
            CompetenceProjects = new List<CompetenceProject>();
            ProjectUsers = new List<ProjectUser>();
            ProjectTowns = new List<ProjectTown>();
            Images = new List<Image>();
        }

        public Project(string name, string description, bool? payment, int companyId) : this()
        {
            Name = name;
            Description = description;
            Payment = payment;
            CompanyId = companyId;
        }


        //НЕ загрузит их в сущность(проекта), только добавит в бд
        public async Task<List<Image>> AddImagesToDbSystem(ApplicationDbContext db, IHostingEnvironment appEnvironment, IFormFile[] images)
        {
            List<Image> res = new List<Image>();
            if (images == null)
                return res;
            foreach (var i in images)
            {
                if (i == null)
                    continue;
                //i.FileName
                byte[] bytes = null;
                using (var binaryReader = new BinaryReader(i.OpenReadStream()))
                {
                    bytes = binaryReader.ReadBytes((int)i.Length);
                }
                bool isImage = Image.IsImage(bytes);
                if (!isImage)
                    continue;
                var fileName = ContentDispositionHeaderValue.Parse(i.ContentDisposition).FileName.Trim('"');
                //получаем формат
                var FileExtension = Path.GetExtension(fileName);
                
                Image img = new Image() { ProjectId = this.Id };
                db.Images.Add(img);
                await db.SaveChangesAsync();
                string shortPath = "/images/uploads/project_" + this.Id + "_" + img.Id + FileExtension;
                img.Path = shortPath;
                await db.SaveChangesAsync();
                res.Add(img);

                
                using (var fileStream = new FileStream(appEnvironment.WebRootPath + shortPath, FileMode.Create))
                {
                    await fileStream.WriteAsync(bytes);
                    // await uploadedFile[0].CopyToAsync(fileStream);
                }
            }


            ////Tuple<>
            
            ////var imgs = Image.GetBytes(images);
            //if (images != null && images.Count > 0)
            //{

            //    foreach (var i in images)
            //        res.Add(new Image() { ProjectId = this.Id });
            //    db.Images.AddRange(res);
            //    await db.SaveChangesAsync();

            //    for (var i = 0; i < images.Count; ++i)
            //    {
            //        string path = "/images/uploads/project_" + this.Id + "_" + res[i].Id;// + uploadedFile[0].FileName;
            //                                                                             // сохраняем файл в папку Files в каталоге wwwroot
            //        using (var fileStream = new FileStream(appEnvironment.WebRootPath + path, FileMode.Create))
            //        {
            //            await images[i].CopyToAsync(fileStream);
            //        }
            //    }

            //}

            return res;
        }

        public async  Task<string> GetCompanyName(ApplicationDbContext db)
        {
            return (await Company.Get(db, this.CompanyId)).Name;
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


        public async static Task<List<int>> GetImagesId(ApplicationDbContext db,int projectId)
        {
            return await db.Images.Where(x1 => x1.ProjectId == projectId).Select(x1 => x1.Id).ToListAsync();
        }

        public async static Task<List<ImageShort>> GetImagesShort(ApplicationDbContext db, int projectId)
        {
            return await db.Images.Where(x1 => x1.ProjectId == projectId).Select(x1 => new ImageShort(x1)).ToListAsync();
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


        public async static Task<Project> Get(ApplicationDbContext db, int? id)
        {
            if (id == null)
                return null;
            return await db.Projects.FirstOrDefaultAsync(x1 => x1.Id == id);
        }

        //не создает запись если ее нет,нет валидации
        public async Task<bool?> ChangeStatusUser(ApplicationDbContext db, StatusInProject newStatus, string userId)
        {
            var record = await db.Entry(this).Collection(x1 => x1.ProjectUsers).Query().FirstOrDefaultAsync(x1 => x1.UserId == userId);
            if (record == null)
                return false;
            if (!await record.ChangeStatus(db, newStatus))
                return null;
            return true;
        }

        //измнение руководителем, дополнительная валидация, валидация только на изменяЕМЫЙ статус
        public async Task<bool?> ChangeStatusUserByLead(ApplicationDbContext db, StatusInProject newStatus, string userId)
        {
            var record = await db.Entry(this).Collection(x1 => x1.ProjectUsers).Query().FirstOrDefaultAsync(x1 => x1.UserId == userId);
            if (record == null)
                return false;
            if (record.Status == StatusInProject.CanceledByStudent || record.Status == StatusInProject.Empty)
                return false;
            if (!await record.ChangeStatus(db, newStatus))
                return null;
            return true;
        }


        //создает если надо и изменяет запись .(если записи нет то создаст ее.) у записи будет переданный статус, нет валидации
        //null-если ошибка 
        public async Task<bool?> CreateChangeStatusUser(ApplicationDbContext db, StatusInProject newStatus, string userId)
        {
            bool exists = false;
            var record = await db.Entry(this).Collection(x1 => x1.ProjectUsers).Query().FirstOrDefaultAsync(x1 => x1.UserId == userId);
            if (record == null)
            {
                db.ProjectUsers.Add(new ProjectUser(userId, this.Id, newStatus));
                await db.SaveChangesAsync();
            }
            else
            {
                exists = true;
                if (!await record.ChangeStatus(db, newStatus))
                    return null;
            }
          

            return exists;
        }


        public async static Task<List<CompetenceShort>> GetCompetences(ApplicationDbContext db,int projectId)
        {
            return await db.CompetenceProjects.Where(x1 => x1.ProjectId == projectId).
                 Join(db.Competences, x1 => x1.CompetenceId, x2 => x2.Id, (x1, x2) => new CompetenceShort(x2)).ToListAsync();
        }

        //добавление без валидации
        public async Task<List<Competence>> AddCompetences(ApplicationDbContext db, string[] competences)
        {
            var needAdded = await Competence.CreateInDbIfNeed(db,competences);

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


        //составляем запрос, данные не учитывая статус проекта
        public static IQueryable<Project> GetActualQueryEntity(ApplicationDbContext db, int? townId)//#TODO можно вынести db.Projects в параметры а этот метод сделать оболочкой, если нужно будет
        {
            //return db.ProjectTowns.Where(x1 => townId == null ? true : (x1.TownId == townId)).
            //    GroupJoin(db.ProjectUsers, x1 => x1.ProjectId, x2 => x2.ProjectId, (x, y) => new { proj = x, prUser = y }).
            //    SelectMany(x => x.prUser.DefaultIfEmpty(), (x, y) => x.proj.ProjectId).GroupBy(x1 => x1).Select(x1 => new { projId = x1.Key, count = x1.Count() }).
            //    Join(db.Projects, x1 => x1.projId, x2 => x2.Id, (x1, x2) => new { proj = x2, count = x1.count }).
            //    OrderByDescending(x1 => x1.count).Select(x1 => x1.proj);

            return db.ProjectTowns.Where(x1 => townId == null ? true : (x1.TownId == townId)).
             GroupJoin(db.ProjectUsers, x1 => x1.ProjectId, x2 => x2.ProjectId, (x, y) => new { projTown = x, prUser = y }).
             SelectMany(x => x.prUser.DefaultIfEmpty(), (x, y) => new { x.projTown.ProjectId, y.Id }).GroupBy(x1 => x1.ProjectId).
             Join(db.Projects, x1 => x1.Key, x2 => x2.Id, (x1, x2) => new { proj = x2, count = x1.Count() }).OrderBy(x1 => x1.count).Select(x1 => x1.proj);

        }
        //аналогично методу GetActualQueryEntity, но без закрытых проектов
        public static IQueryable<Project> GetActualQueryEntityWithStatus(ApplicationDbContext db, int? townId)
        {
            return  Project.GetActualQueryEntity(db, townId).Where(x1=>x1.Status!=StatusProject.Closed&& x1.Status!=StatusProject.Complited);
        }

        //получаем полные данные, не учитывая статус проекта
        public async static Task<List<Project>> GetActualEntity(ApplicationDbContext db, int? townId)
        {
            return await Project.GetActualQueryEntity(db, townId).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }
        //получаем сокращенные данные, не учитывая статус проекта
        public async static Task<List<ProjectShort>> GetActualShortEntityWithStatus(ApplicationDbContext db, int? townId, string userId)
        {
            var projs = await Project.GetActualQueryEntityWithStatus(db, townId).Select(x1 => new ProjectShort(x1.Name, x1.Id)).ToListAsync();
            await ProjectShort.SetMainImages(db, projs);
            if (string.IsNullOrWhiteSpace(userId))
                return projs;
            await Project.SetUserStatusInProject(db, projs, userId);
            //var status = await db.ProjectUsers.Where(x1 => x1.UserId == userId && projs.FirstOrDefault(x2 => x2.ProjectId == x1.ProjectId) != null).ToListAsync();
            //status.ForEach(x1 =>
            //{
            //    var tmp = projs.FirstOrDefault(x2 => x2.ProjectId == x1.ProjectId);
            //    if (tmp != null)
            //        tmp.Status = x1.Status;
            //});
            return projs;
        }

        public async static Task<List<ProjectShort>> GetByStartName(ApplicationDbContext db, int? townId, string userId,string name)
        {
            var projs = await Project.GetActualQueryEntity(db, townId).Where(x1=>x1.Name.Contains(name)).Select(x1 => new ProjectShort(x1.Name, x1.Id)).ToListAsync();
            await ProjectShort.SetMainImages(db, projs);
            if (string.IsNullOrWhiteSpace(userId))
                return projs;
            await Project.SetUserStatusInProject(db,projs,userId);

            //var status = await db.ProjectUsers.Where(x1 => x1.UserId == userId && projs.FirstOrDefault(x2 => x2.ProjectId == x1.ProjectId) != null).ToListAsync();
            //status.ForEach(x1 =>
            //{
            //    var tmp = projs.FirstOrDefault(x2 => x2.ProjectId == x1.ProjectId);
            //    if (tmp != null)
            //        tmp.Status = x1.Status;
            //});
            return projs;
        }


        public async static Task SetUserStatusInProject(ApplicationDbContext db, List<ProjectShort> projects, string userId)
        {
            var status = await db.ProjectUsers.Where(x1 => x1.UserId == userId && projects.FirstOrDefault(x2 => x2.ProjectId == x1.ProjectId) != null).ToListAsync();
            status.ForEach(x1 =>
            {
                var tmp = projects.FirstOrDefault(x2 => x2.ProjectId == x1.ProjectId);
                if (tmp != null)
                    tmp.Status = x1.Status;
            });
        }


            public async static Task<List<int>> SortByActual(ApplicationDbContext db)
        {
            return await db.ProjectUsers.GroupBy(x1 => x1.ProjectId).OrderBy(x1 => x1.Count()).Select(x1 => x1.Key).ToListAsync();//Select(x1=>new { x1.Key,Count= x1.Count() })
        }


        public async static Task<List<int>> GetActualInTown(ApplicationDbContext db, int townId)
        {
            return await Project.SortByActual(db, await Project.GetByTown(db, townId));
        }


        public async static Task<List<TownShort>> GetTownsShort(ApplicationDbContext db, int projectId)
        {
            return await db.ProjectTowns.Where(x1 => x1.ProjectId == projectId).
                Join(db.Towns, x1 => x1.TownId, x2 => x2.Id, (x1, x2) => new TownShort(x2)).ToListAsync();
        }


        //без сортировки, просто все записи в городе
        public async static Task<List<int>> GetByTown(ApplicationDbContext db, int townId)
        {
            return await db.ProjectTowns.Where(x1 => x1.TownId == townId).Select(x1 => x1.ProjectId).Distinct().ToListAsync();
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

        public async Task<bool> CheckAccess(ApplicationDbContext db, string userId)
        {
            return await Company.CheckAccess(db,userId,this.CompanyId);
        }

        }
}
