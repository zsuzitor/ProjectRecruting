using ProjectRecruting.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.ShortModel
{
    public class TownShort
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TownShort()
        {

        }
        public TownShort(int id, string name) : this()
        {
            Id = id;
            Name = name;
        }
        public TownShort(Town town)
        {
            Id = town.Id;
            Name = town.Name;
        }
    }
}
