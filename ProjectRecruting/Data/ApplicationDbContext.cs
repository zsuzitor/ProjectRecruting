using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectRecruting.Models;
using ProjectRecruting.Models.Domain;
using ProjectRecruting.Models.Domain.ManyToMany;

namespace ProjectRecruting.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<Company> Companys { get; set; }
        public DbSet<Competence> Competences { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Town> Towns { get; set; }
        public DbSet<Image> Images { get; set; }

        public DbSet<CompanyUser> CompanyUsers { get; set; }
        public DbSet<CompetenceProject> CompetenceProjects { get; set; }
        public DbSet<ProjectTown> ProjectTowns { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<CompetenceUser> CompetenceUsers { get; set; }
        

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
           // Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // использование Fluent API см по тегу fluent
            modelBuilder.Entity<Company>()
            .HasMany(x => x.Projects)
            .WithOne(x => x.Company);

            modelBuilder.Entity<Project>()
            .HasOne(x => x.Company)
            .WithMany(x => x.Projects);



            modelBuilder.Entity<ProjectUser>()
             .HasOne(x => x.Project)
            .WithMany(x => x.ProjectUsers);
            modelBuilder.Entity<ProjectUser>()
             .HasOne(x => x.User)
            .WithMany(x => x.ProjectUsers);

            modelBuilder.Entity<CompetenceUser>()
             .HasOne(x => x.User)
            .WithMany(x => x.CompetenceUsers);
            modelBuilder.Entity<CompetenceUser>()
             .HasOne(x => x.Competence)
            .WithMany(x => x.CompetenceUsers);

            

            modelBuilder.Entity<ProjectTown>()
            .HasOne(x => x.Project)
           .WithMany(x => x.ProjectTowns);
            modelBuilder.Entity<ProjectTown>()
             .HasOne(x => x.Town)
            .WithMany(x => x.ProjectTowns);

            modelBuilder.Entity<CompetenceProject>()
            .HasOne(x => x.Project)
            .WithMany(x => x.CompetenceProjects);
            modelBuilder.Entity<CompetenceProject>()
           .HasOne(x => x.Competence)
           .WithMany(x => x.CompetenceProjects);

            modelBuilder.Entity<CompanyUser>()
           .HasOne(x => x.User)
           .WithMany(x => x.CompanyUsers);
            modelBuilder.Entity<CompanyUser>()
           .HasOne(x => x.Company)
           .WithMany(x => x.CompanyUsers);


            base.OnModelCreating(modelBuilder);
        }



    }
}
