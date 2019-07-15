﻿using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using CookBook.CoreProject.Helpers;
using CookBook.Domain.ResultDtos;
using CookBook.Domain.ResultDtos.RecipeDtos;
using CookBook.Domain.ViewModels.RecipeViewModels;
using CookBook.Presentation.Controllers;
using CookBook.Presentation.ObjectResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Testing.Helpers;
using Testing.Mocking;
using Xunit;

namespace Testing.TestSuites
{
    public class RecipeControllerTesting
    {
        private readonly RecipeController _controller;
        private readonly ActionExecutingContext _context;

        public RecipeControllerTesting()
        {
            var mocker = new RecipeControllerMocking();
            _controller = mocker.Setup();
            _context = mocker.SetupContext(_controller);
        }

        [Fact]
        public async Task CreateRecipeOk()
        {
            var model = CreateDefaultCreateRecipeViewModel();
            var json = (OkObjectResult)await _controller.CreateRecipe(model);
            var result = (RecipeDto)json.Value;
            Assert.NotNull(result);
        }

        private CreateUpdateRecipeViewModel CreateDefaultCreateRecipeViewModel()
        {
            return new CreateUpdateRecipeViewModel
            {
                Name = "Name",
                Description = "A short description",
                Content = "A very very long description",
                Tags = new List<string> { "tag1", "tag2" }
            };
        }

        [Fact]
        public void CreateRecipeHasModelValidation()
        {
            Assert.True(AttributeHelper.IsModelValidationApplied(typeof(RecipeController), "CreateRecipe"));
        }

        [Fact]
        public void CreateRecipeInvalidModel()
        {
            var model = new CreateUpdateRecipeViewModel();
            _context.ModelState.Validate(model);

            var error = AttributeHelper.ExecuteModelValidation(_context);

            Assert.Equal((int)HttpStatusCode.BadRequest, error.Code);
            Assert.Contains("The Name field is required.", error.Errors);
            Assert.Contains("The Description field is required.", error.Errors);
            Assert.Contains("The Content field is required.", error.Errors);
        }

        [Fact]
        public async Task GetRecipeOk()
        {
            var json = (OkObjectResult)await _controller.GetRecipe(5);
            var result = (RecipeDto) json.Value;

            Assert.Equal(5, result.Id);
        }

        [Fact]
        public async Task GetRecipeNotFound()
        {
            var json = (NotFoundObjectResult)await _controller.GetRecipe(-5);
            var error = (ErrorDto)json.Value;

            Assert.Equal((int)HttpStatusCode.NotFound, error.Code);
            Assert.Contains("Recipe not found.", error.Errors);
        }

        [Fact]
        public async Task UpdateRecipeOk()
        {
            var model = CreateDefaultCreateRecipeViewModel();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SetupClaimsPrincipal() }
            };

            var json = (OkObjectResult) await _controller.UpdateRecipe(model, 1);
            var result = (RecipeDto) json.Value;

            Assert.NotNull(result);
        }

        private ClaimsPrincipal SetupClaimsPrincipal(string role = "User")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task UpdateRecipeWrongUserId()
        {
            var model = CreateDefaultCreateRecipeViewModel();
            var user = SetupClaimsPrincipal();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var json = (ForbiddenObjectResult)await _controller.UpdateRecipe(model, 2);
            var error = (ErrorDto)json.Value;

            Assert.Equal((int) HttpStatusCode.Forbidden, error.Code);
            Assert.Contains("User id does not match.", error.Errors);
        }

        [Fact]
        public async Task UpdateRecipeNotFound()
        {
            var model = CreateDefaultCreateRecipeViewModel();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SetupClaimsPrincipal() }
            };

            var json = (NotFoundObjectResult)await _controller.UpdateRecipe(model, 0);
            var error = (ErrorDto)json.Value;

            Assert.Equal((int)HttpStatusCode.NotFound, error.Code);
            Assert.Contains("Recipe not found.", error.Errors);
        }

        [Fact]
        public void UpdateRecipeHasModelValidation()
        {
            Assert.True(AttributeHelper.IsModelValidationApplied(typeof(RecipeController), "UpdateRecipe"));
        }
    }
}