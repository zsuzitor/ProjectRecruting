using ProjectRecruting.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.ShortModel
{
    public class CompetenceShort
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public CompetenceShort()
        {

        }
        public CompetenceShort(string name, int id) : this()
        {
            Name = name;
            Id = id;
        }
        public CompetenceShort(Competence competence) : this()
        {
            Name = competence.Name;
            Id = competence.Id;
        }
    }
}
