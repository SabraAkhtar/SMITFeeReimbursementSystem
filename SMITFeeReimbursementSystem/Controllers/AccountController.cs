using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.Services;
using SMITFeeReimbursementSystem.ViewModels;

namespace SMITFeeReimbursementSystem.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IUserRegistrationService registrationService,
    IAuthRedirectService authRedirectService,
    ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
            return View(model);
        }

        if (result.Succeeded)
        {
            return await RedirectAfterAuthAsync(user, returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register()
    {
        ViewBag.IsFirstUser = await registrationService.IsFirstUserAsync();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var isFirstUser = await registrationService.IsFirstUserAsync();
        ViewBag.IsFirstUser = isFirstUser;

        if (!isFirstUser && !AppRoles.Registerable.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Invalid role selected.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FullName = model.FullName,
            RollNumber = model.RollNumber
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        var role = isFirstUser
            ? AppRoles.Admin
            : await registrationService.ResolveRoleForNewUserAsync(model.Role);
        await userManager.AddToRoleAsync(user, role);

        if (isFirstUser)
        {
            logger.LogInformation("First registered user '{Email}' assigned Admin role.", model.Email);
            TempData["StatusMessage"] = "You are the first user and have been assigned the Admin role.";
        }

        await signInManager.SignInAsync(user, isPersistent: false);

        return await RedirectAfterAuthAsync(user, returnUrl: null);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        // Always show success to prevent email enumeration
        if (user is null)
        {
            TempData["StatusMessage"] = "If that email is registered, a reset link has been sent.";
            return RedirectToAction(nameof(Login));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = Url.Action(nameof(ResetPassword), "Account",
            new { email = model.Email, token }, Request.Scheme);

        // In production, send email. For now, store in TempData for demo.
        TempData["ResetLink"] = resetUrl;
        TempData["StatusMessage"] = "Password reset link generated. Check the demo link below.";
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation() => View();

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (email is null || token is null) return RedirectToAction(nameof(Login));
        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            TempData["StatusMessage"] = "Password reset successful.";
            return RedirectToAction(nameof(Login));
        }

        var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["StatusMessage"] = "Password reset successfully. You can now log in.";
        return RedirectToAction(nameof(Login));
    }

    private async Task<IActionResult> RedirectAfterAuthAsync(ApplicationUser user, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        var homePath = await authRedirectService.GetHomePathForUserAsync(user);
        return Redirect(homePath);
    }
}
