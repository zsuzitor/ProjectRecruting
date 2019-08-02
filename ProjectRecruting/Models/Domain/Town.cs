using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    //должен быть предопределенный город "удаленка" #TODO
    public class Town
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public List<ProjectTown> ProjectTowns { get; set; }
        public Town()
        {

        }

        public Town(string name)
        {
            Name = name;
        }

        public async static Task<Town> GetByName(ApplicationDbContext db,string name)
        {
            return await db.Towns.FirstOrDefaultAsync(x1 => x1.Name == name);
        }

        public async static Task<List<Town>> GetOrCreate(ApplicationDbContext db, List<string> townNames)
        {
            if (townNames == null || townNames.Count == 0)
                return new List<Town>();
            for (int i=0;i< townNames.Count;++i)
            {
                townNames[i] = townNames[i].ToLower().Trim();
            }
            var townsExists= await db.Towns.Where(x1=> townNames.Contains(x1.Name)).ToListAsync();
            List<Town> notExist = new List<Town>();
            foreach (var i in townNames)
            {
                if (townsExists.FirstOrDefault(x1 => x1.Name == i) != null)
                    continue;
                notExist.Add(new Town(i));

            }
            db.Towns.AddRange(notExist);
            await db.SaveChangesAsync();
            notExist.AddRange(townsExists);

            return notExist;
        }


    }
}
