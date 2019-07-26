using Microsoft.CodeAnalysis;
using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Company
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Не указано имя")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Number { get; set; }
        public string Email { get; set; }

        public byte[] Image { get; set; }

        //люди которые могут изменять компанию
        public List<CompanyUser> CompanyUsers { get; set; }
        public List<Project> Projects { get; set; }

        public Company()
        {
            Image = null;
        }
        public Company(string name,string description,string number,string email)
        {
            Name = name;
            Description = description;
            Number = number;
            Email = email;

        }

        public void ChangeData(string name, string description, string number, string email,byte[] image)
        {
            Name = name;
            Description = description;
            Number = number;
            Email = email;
            if (image != null)
                Image = image;

        }
    }
}
