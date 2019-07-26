using Microsoft.AspNetCore.Http;
using ProjectRecruting.Models.Domain.ManyToMany;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.Domain
{
    public class Project
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Не указано имя")]
        public string Name { get; set; }
        public string Description { get; set; }

        public bool? Payment { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }


        public List<CompetenceProject> CompetenceProjects { get; set; }
        public List<ProjectUser> ProjectUsers { get; set; }
        //если пусто то видно всем
        public List<ProjectTown> ProjectTowns { get; set; }

        public List<Image> Images { get; set; }

        public Project()
        {

        }

        public Project(string name, string description, bool? payment, int companyId)
        {
            Name = name;
            Description = description;
            Payment = payment;
            CompanyId = companyId;
        }


        public void SetImages(IFormFileCollection images)
        {
            var imgs = Image.GetBytes(images);
            foreach (var i in imgs)
            {
                this.Images.Add(new Image() { ProjectId = this.Id, Data = i });


            }
        }


    }
}
