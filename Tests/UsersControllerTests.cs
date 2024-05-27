using ImageService.Controllers;
using ImageService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Tests;

public class UsersControllerTests
{
    [Fact]
    public async Task GetUsers_ReturnsListOfUsers()
    {
        var options = new DbContextOptionsBuilder<ImageServiceContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        
        await using (var context = new ImageServiceContext(options))
        {
            context.Users.AddRange(GetTestUsers());
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new ImageServiceContext(options))
        {
            var controller = new UsersController(context);
            var result = await controller.GetUsers();

            // Assert
            Assert.NotNull(result);
            var users = Assert.IsType<List<User>>(result.Value);
            Assert.Equal(3, users.Count);
        }
    }

    [Fact]
    public async Task GetUser_ReturnsUser_WhenUserExists()
    {
        var options = new DbContextOptionsBuilder<ImageServiceContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        
        using (var context = new ImageServiceContext(options))
        {
            context.Users.AddRange(GetTestUsers());
            context.SaveChanges();
        }

        // Act
        using (var context = new ImageServiceContext(options))
        {
            var controller = new UsersController(context);
            var result = await controller.GetUser(1);

            // Assert
            Assert.NotNull(result);
            var user = Assert.IsType<User>(result.Value);
            Assert.Equal(1, user.Id);
        }
    }

    [Fact]
    public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var options = new DbContextOptionsBuilder<ImageServiceContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        // Act
        using (var context = new ImageServiceContext(options))
        {
            var controller = new UsersController(context);
            var result = await controller.GetUser(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }

    [Fact]
    public async Task PostUser_AddsNewUser()
    {
        var options = new DbContextOptionsBuilder<ImageServiceContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        var newUser = new User { Id = 4, Name = "user4" };

        // Act
        using (var context = new ImageServiceContext(options))
        {
            var controller = new UsersController(context);
            var result = await controller.PostUser(newUser);

            // Assert
            Assert.NotNull(result);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var user = Assert.IsType<User>(createdAtActionResult.Value);
            Assert.Equal(newUser.Id, user.Id);
            Assert.Equal(newUser.Name, user.Name);
            
            using (var dbContext = new ImageServiceContext(options))
            {
                Assert.Equal(4, dbContext.Users.Count());
            }
        }
    }

    [Fact]
    public async Task DeleteUser_RemovesUser()
    {
        var options = new DbContextOptionsBuilder<ImageServiceContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        
        using (var context = new ImageServiceContext(options))
        {
            context.Users.AddRange(GetTestUsers());
            context.SaveChanges();
        }

        // Act
        using (var context = new ImageServiceContext(options))
        {
            var controller = new UsersController(context);
            var result = await controller.DeleteUser(1);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
            
            using (var dbContext = new ImageServiceContext(options))
            {
                Assert.Equal(2, dbContext.Users.Count());
                Assert.Null(dbContext.Users.FirstOrDefault(u => u.Id == 1));
            }
        }
    }

    private List<User> GetTestUsers()
    {
        return new List<User>
        {
            new User { Id = 1, Name = "user1" },
            new User { Id = 2, Name = "user2" },
            new User { Id = 3, Name = "user3" }
        };
    }
}
