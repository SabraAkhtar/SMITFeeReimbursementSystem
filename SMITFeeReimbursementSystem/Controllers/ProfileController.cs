using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMITFeeReimbursementSystem.Models;
using SMITFeeReimbursementSystem.ViewModels;

namespace SMITFeeReimbursementSystem.Controllers;

[Authorize]
public class ProfileController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var roles = await userManager.GetRolesAsync(user);
        var model = new EditProfileViewModel
        {
            FullName = user.FullName ?? "",
            RollNumber = user.RollNumber,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            Role = roles.FirstOrDefault()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EditProfileViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var roles = await userManager.GetRolesAsync(user);
        model.Email = user.Email;
        model.Role = roles.FirstOrDefault();

        if (!ModelState.IsValid) return View(model);

        user.FullName = model.FullName;
        user.RollNumber = model.RollNumber;
        user.PhoneNumber = model.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["StatusMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["StatusMessage"] = "Password changed successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ChangeEmail()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        ViewBag.CurrentEmail = user.Email;
        return View(new ChangeEmailViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        ViewBag.CurrentEmail = user.Email;

        if (!ModelState.IsValid) return View(model);

        // Check if new email already taken
        var existing = await userManager.FindByEmailAsync(model.NewEmail);
        if (existing is not null && existing.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.NewEmail), "This email is already in use.");
            return View(model);
        }

        user.Email = model.NewEmail;
        user.UserName = model.NewEmail;
        user.NormalizedEmail = model.NewEmail.ToUpperInvariant();
        user.NormalizedUserName = model.NewEmail.ToUpperInvariant();
        user.EmailConfirmed = true;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["StatusMessage"] = "Email changed successfully.";
        return RedirectToAction(nameof(Index));
    }
}
