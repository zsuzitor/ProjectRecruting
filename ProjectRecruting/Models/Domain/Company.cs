using Microsoft.CodeAnalysis;
using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Number { get; set; }
        public string Email { get; set; }


        public List<CompanyUser> CompanyUsers { get; set; }
        public List<Project> Projects { get; set; }

        public Company()
        {

        }
    }
}
