using Microsoft.AspNetCore.Identity;
using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string SurName { get; set; }


        public List<CompanyUser> CompanyUsers { get; set; }

        public List<ProjectUser> ProjectUsers { get; set; }

        public ApplicationUser()
        {

        }

    }
}
