﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using E_Shop_Engine.Domain.DomainModel.IdentityModel;
using E_Shop_Engine.Services.Services;
using E_Shop_Engine.Website.Models;
using Microsoft.AspNet.Identity;
using Moq;
using NUnit.Framework;

namespace E_Shop_Engine.UnitTests.E_Shop_Engine.Website.UnitTests.Controllers.AccountController
{
    public class ChangePasswordMethodTests : AccountControllerBaseTest<UserChangePasswordViewModel>
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _model = new UserChangePasswordViewModel();
        }

        [Test(Description = "HTTPGET")]
        public void ChangePassword_WhenCalled_ReturnsViewWithForm()
        {
            ActionResult result = _controller.ChangePassword();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test(Description = "HTTPPOST")]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase(" ", "")]
        [TestCase("", " ")]
        [TestCase("a", "a")]
        [TestCase("a", "b")]
        [TestCase("ab", "abc")]
        [TestCase("abcdef", "abcdefg")]
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public void ChangePassword_WhenNotValidModelPassed_ValidationFails(
          string newPw,
          string newPwCopy)
        {
            UserChangePasswordViewModel model = new UserChangePasswordViewModel()
            {
                NewPassword = newPw,
                NewPasswordCopy = newPwCopy,
                OldPassword = "abcdef"
            };

            IsModelStateValidationWorks(model);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenModelStateHasError_ReturnsViewWithModelError()
        {
            AddModelStateError("test");

            ActionResult result = await _controller.ChangePassword(_model);
            IEnumerable<bool> errors = GetErrorsWithMessage("test");

            AssertReturnsViewWithModelError(result, errors);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenValidModelPassed_ShouldRedirectToIndex()
        {
            SetupMockedWhenValidModelPassed();

            ActionResult result = await _controller.ChangePassword(_model);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual("Index", (result as RedirectToRouteResult).RouteValues["action"]);
        }

        protected override void SetupMockedWhenValidModelPassed()
        {
            SetupFindById(_user);
            _userManager.Setup(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(true);
            _userManager.Setup(um => um.PasswordValidator.ValidateAsync(It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(um => um.PasswordHasher.HashPassword(It.IsAny<string>())).Returns(It.IsAny<string>());
            _userManager.Setup(um => um.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenValidModelPassed_CheckPasswordMethodCall()
        {
            SetupMockedWhenValidModelPassed();

            ActionResult result = await _controller.ChangePassword(_model);

            _userManager.Verify(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Once);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenValidModelPassed_PasswordValidationMethodCall()
        {
            SetupMockedWhenValidModelPassed();

            ActionResult result = await _controller.ChangePassword(_model);

            _userManager.Verify(um => um.PasswordValidator.ValidateAsync(It.IsAny<string>()), Times.Once);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenValidModelPassed_HashPasswordMethodCall()
        {
            SetupMockedWhenValidModelPassed();

            ActionResult result = await _controller.ChangePassword(_model);

            _userManager.Verify(um => um.PasswordHasher.HashPassword(It.IsAny<string>()), Times.Once);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenValidModelPassed_UpdateMethodCall()
        {
            SetupMockedWhenValidModelPassed();

            ActionResult result = await _controller.ChangePassword(_model);

            _userManager.Verify(um => um.UpdateAsync(It.IsAny<AppUser>()), Times.Once);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenNewPasswordAndConfirmationPasswordDoesntMatch_ReturnsViewWithModelError()
        {
            UserChangePasswordViewModel model = new UserChangePasswordViewModel()
            {
                NewPassword = "",
                NewPasswordCopy = "a"
            };

            ActionResult result = await _controller.ChangePassword(model);
            IEnumerable<bool> errors = GetErrorsWithMessage(ErrorMessage.PasswordsDontMatch);

            AssertReturnsViewWithModelError(result, errors, model);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenUserNotFound_ReturnsViewWithModelError()
        {
            SetupFindById();

            ActionResult result = await _controller.ChangePassword(_model);
            IEnumerable<bool> errors = GetErrorsWithMessage(ErrorMessage.NullUser);

            AssertReturnsViewWithModelError(result, errors);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenUsersPasswordNotValid_ReturnsViewWithModelError()
        {
            _userManager.Setup(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(false);

            ActionResult result = await _controller.ChangePassword(_model);
            IEnumerable<bool> errors = GetErrorsWithMessage(ErrorMessage.PasswordNotValid);

            AssertReturnsViewWithModelError(result, errors);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenNewPasswordNotValid_ReturnsViewWithModelError()
        {
            SetupFindById(_user);
            _userManager.Setup(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(true);
            _userManager.Setup(um => um.PasswordValidator.ValidateAsync(It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed("test"));

            ActionResult result = await _controller.ChangePassword(_model);
            IEnumerable<bool> errors = GetErrorsWithMessage("test");

            AssertReturnsViewWithModelError(result, errors);
        }

        [Test(Description = "HTTPPOST")]
        public async Task ChangePassword_WhenUpdatingFailed_ReturnsViewWithModelError()
        {
            SetupFindById(_user);
            _userManager.Setup(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(true);
            _userManager.Setup(um => um.PasswordValidator.ValidateAsync(It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(um => um.PasswordHasher.HashPassword(It.IsAny<string>())).Returns(It.IsAny<string>());
            _userManager.Setup(um => um.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Failed("test"));

            ActionResult result = await _controller.ChangePassword(_model);
            IEnumerable<bool> errors = GetErrorsWithMessage("test");

            AssertReturnsViewWithModelError(result, errors);
        }
    }
}
