using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain.ManyToMany
{
    public class CompanyUser
    {
        public int Id { get; set; }


        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

    }
}
