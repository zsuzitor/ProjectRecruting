using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain.ManyToMany
{
    public class CompetenceUser
    {
        public int Id { get; set; }

        public int CompetenceId { get; set; }
        public Competence Competence { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }


        public CompetenceUser()
        {

        }

        public CompetenceUser(int competenceId, string userId) : this()
        {
            CompetenceId = competenceId;
            UserId = userId;
        }



    }
}
