﻿using System;
using System.Collections.Generic;
using System.Linq;
using CookBook.CoreProject.Constants;
using CookBook.CoreProject.Interfaces;
using CookBook.Domain.Mappers;
using CookBook.Domain.Models;
using CookBook.Presentation.Controllers;
using CookBook.Presentation.JWT;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Testing.Mocking
{
    class AccountMocking : BaseMocking<AccountController, AccountProfile>
    {
        private readonly List<ApplicationUser> _users = new List<ApplicationUser>();

        private readonly ApplicationUser _defaultUser = new ApplicationUser
        {
            UserName = "user1",
            Email = "user1@mailinator.com",
            PasswordHash = "pass"
        };

        public Mock<UserManager<ApplicationUser>> MockedUserManager { get; private set; }

        public Mock<ICookieService> MockedCookieService { get; private set; }

        public override AccountController Setup()
        {
            MockedUserManager = MockUserManager();
            MockedCookieService = MockCookieService();
            return new AccountController(MockedUserManager.Object, SetupMapper(), new JwtFactory("https://localhost:44342/", "gyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy", 60),
                new RefreshTokenFactory(128, 60), MockedCookieService.Object);
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
            manager.Setup(m => m.AddToRoleAsync(It.Is<ApplicationUser>(u => UserExists(u)), UserRoleNames.User))
                .ReturnsAsync(IdentityResult.Success);
        }

        private void MockLoginMethods(Mock<UserManager<ApplicationUser>> manager)
        {
            manager.Setup(m => m.FindByNameAsync("user1"))
                .ReturnsAsync(_defaultUser);
            manager.Setup(m => m.FindByEmailAsync("user1@mailinator.com"))
                .ReturnsAsync(_defaultUser);
            manager.Setup(m => m.CheckPasswordAsync(_defaultUser, "pass"))
                .ReturnsAsync(true);
            manager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>());
        }

        private bool UserExists(ApplicationUser u)
        {
            return _users.Select(user => user.UserName.ToLower()).Any(name => name == u.UserName.ToLower())
                   || _users.Select(user => user.Email.ToLower()).Any(name => name == u.Email.ToLower());
        }

        private Mock<ICookieService> MockCookieService()
        {
            var mock = new Mock<ICookieService>();
            var token = "token";
            mock.Setup(m => m.WriteHttpOnlyCookie(It.IsAny<string>(), token, It.IsAny<DateTime?>()));
            mock.Setup(m => m.TryGetCookie(It.IsAny<string>(), out token)).Returns(true);
            return mock;
        }
    }
}
