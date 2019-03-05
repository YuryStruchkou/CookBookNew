﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CookBook.Domain.Mappers;
using CookBook.Domain.Models;
using CookBook.Presentation.Controllers;
using CookBook.Presentation.JWT;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ControllerTesting.Mocking
{
    class AccountMocking
    {
        private List<ApplicationUser> _users = new List<ApplicationUser>();

        private readonly ApplicationUser DefaultUser = new ApplicationUser
        {
            UserName = "user1",
            Email = "user1@mailinator.com",
            PasswordHash = "pass"
        };

        public ActionExecutingContext SetupContext(AccountController controller)
        {
            return new ActionExecutingContext(new ControllerContext(new ActionContext(new Mock<HttpContext>().Object, new RouteData(),
                new ControllerActionDescriptor())), new List<IFilterMetadata>(),  new Dictionary<string, object>(), controller);
        }

        public AccountController SetupController()
        {
            var userManager = MockUserManager().Object;
            var mapper = SetupMapper();
            return new AccountController(userManager, mapper, new JwtFactory("https://localhost:44342/", "gyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy"));
        }

        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var manager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            manager.Object.UserValidators.Add(new UserValidator<ApplicationUser>());
            manager.Object.PasswordValidators.Add(new PasswordValidator<ApplicationUser>());

            MockRegistrationMethods(manager);
            MockLoginMethods(manager);

            return manager;
        }

        private void MockRegistrationMethods(Mock<UserManager<ApplicationUser>> manager)
        {
            manager.Setup(m => m.CreateAsync(It.Is<ApplicationUser>(u => UserExists(u)), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());
            manager.Setup(m => m.CreateAsync(It.Is<ApplicationUser>(u => !UserExists(u)), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, password) => _users.Add(user));
        }

        private void MockLoginMethods(Mock<UserManager<ApplicationUser>> manager)
        {
            manager.Setup(m => m.FindByNameAsync("user1"))
                .ReturnsAsync(DefaultUser);
            manager.Setup(m => m.FindByEmailAsync("user1@mailinator.com"))
                .ReturnsAsync(DefaultUser);
            manager.Setup(m => m.CheckPasswordAsync(DefaultUser, "pass"))
                .ReturnsAsync(true);
        }

        private bool UserExists(ApplicationUser u)
        {
            return _users.Select(user => user.UserName.ToLower()).Any(name => name == u.UserName.ToLower())
                   || _users.Select(user => user.Email.ToLower()).Any(name => name == u.Email.ToLower());
        }

        private IMapper SetupMapper()
        {
            var config = new MapperConfiguration(c => c.AddProfile<AccountProfile>());
            return config.CreateMapper();
        }
    }
}
