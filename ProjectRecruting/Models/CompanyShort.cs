using ProjectRecruting.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models
{
    public class CompanyShort
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }
        public string Number { get; set; }
        public string Email { get; set; }

        public CompanyShort()
        {

        }

        public CompanyShort(Company company)
        {
            this.Id = company.Id;
            this.Name = company.Name;
            this.Description = company.Description;
            this.Number = company.Number;
            this.Email = company.Email;
        }
    }
}
