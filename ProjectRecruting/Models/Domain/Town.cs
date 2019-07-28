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


        public async static Task<Town> GetByName(ApplicationDbContext db,string name)
        {
            return await db.Towns.FirstOrDefaultAsync(x1 => x1.Name == name);
        }
    }
}
