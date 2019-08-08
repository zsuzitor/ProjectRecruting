using Microsoft.EntityFrameworkCore;
using Moq;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace ProjectRecruting.Tests
{
    public class Class1
    {
        [Fact]
        public void IndexViewDataMessage()
        {
            // Arrange
            Project proj = new Project();

            // Act
            proj.ChangeData("nm","descr",false);

            // Assert

            Assert.Equal("nm", proj.Name);
        }

        [Fact]
        public async void ProjectGet()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                  .Options;
            //var settings = new SqlSettings
            //{
            //    InMemory = true
            //};
            var context = new ApplicationDbContext(options);
            //var context = new Mock<ApplicationDbContext>(options);
            

            //var mock = new Mock<ApplicationDbContext>();
            //mock.Setup(repo => repo.Projects).Returns(GetAllProject());

            //var mockSet = MockHelper.GetMockDbSet(_users);
            var proj=await Project.Get(context,1);
            Assert.Null(proj);
            context.Projects.Add(new Project("123","322",false,1));
            context.SaveChanges();
            proj = await Project.Get(context, 1);
            Assert.NotNull(proj);
            // Act
            //proj.ChangeData("nm", "descr", false);

            //// Assert

            //Assert.Equal("nm", proj.Name);
        }



        //public class test : DbSet<Project>
        //{

        //}

        

        //private DbSet<Project> GetAllProject()
        //{
        //    return new DbSet<Project>() { new Project() };
        //}
    }
}
