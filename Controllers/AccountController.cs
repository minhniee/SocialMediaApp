using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Data;
using SocialMediaApp.Services;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Controllers;

public class AccountController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IAccountService _accountService;
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IWebHostEnvironment environment,
        IEmailService emailService,
        IAccountService accountService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
        _emailService = emailService;
        _accountService = accountService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    public IActionResult CheckEmail()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                JoinDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.Id, code = code }, protocol: Request.Scheme);

                await _emailService.SendEmailAsync(user.Email, "Confirm your account",
                    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                await _signInManager.SignInAsync(user, false);
                return RedirectToAction(nameof(CheckEmail));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }


    [HttpGet]
    public IActionResult RegisterConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please fill in all fields correctly.");
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // Uncomment if email confirmation is required
        if (!user.EmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "You must confirm your email to log in.");
            return View(model);
        }

        // var result = await _signInManager.PasswordSignInAsync(
        //     user.UserName,
        //     model.Password,
        //     model.RememberMe,
        //     lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return RedirectToLocal(returnUrl);
        }
        if (result.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
        }
        if (result.IsLockedOut)
        {
            return RedirectToAction(nameof(Lockout));
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }


    [HttpGet]
    public IActionResult LoginWith2fa(bool rememberMe, string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = _signInManager.GetTwoFactorAuthenticationUserAsync().Result;

        if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

        var model = new LoginWith2faViewModel { RememberMe = rememberMe };
        ViewData["ReturnUrl"] = returnUrl;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

        var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result =
            await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe,
                model.RememberMachine);

        if (result.Succeeded) return RedirectToLocal(returnUrl);

        if (result.IsLockedOut) return RedirectToAction(nameof(Lockout));

        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Lockout()
    {
        return View();
    }

    [HttpGet]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new EditProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth,
            CurrentProfilePictureUrl = user.ProfilePictureUrl
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        try
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;
            user.DateOfBirth = model.DateOfBirth;

            // Handle profile picture upload
            if (model.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + "_" + model.ProfilePicture.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(fileStream);
                }

                user.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
            }

            // Update email if changed
            if (model.Email != user.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }

                // Update username to match email
                var setUsernameResult = await _userManager.SetUserNameAsync(user, model.Email);
                if (!setUsernameResult.Succeeded)
                {
                    foreach (var error in setUsernameResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword);

                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }
            }

            await _userManager.UpdateAsync(user);


            // Refresh sign-in cookie with the updated user info
            await _signInManager.RefreshSignInAsync(user);
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = ex;
        }

        TempData["StatusMessage"] = "Your profile has been updated";
        return RedirectToAction("Index", "Profile");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                //     // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // TODO: Send email with reset link

            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { userId = user.Id, code = token }, protocol: Request.Scheme);

            await _emailService.SendEmailAsync(user.Email, "ResetPassWord", callbackUrl);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string code = null)
    {
        if (code == null) return BadRequest("A code must be supplied for password reset.");

        var model = new ResetPasswordViewModel { Code = code };
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            // Don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded) return RedirectToAction(nameof(ResetPasswordConfirmation));

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null) return RedirectToAction("Index", "Home");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");

        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");

        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ManageAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new ManageAccountViewModel
        {
            Email = user.Email,
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user),
            PhoneNumber = user.PhoneNumber,
            TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null
        };

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactorAuthentication()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(authenticatorKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var model = new EnableAuthenticatorViewModel
        {
            SharedKey = authenticatorKey,
            AuthenticatorUri = GenerateQrCodeUri(user.Email, authenticatorKey)
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableTwoFactorAuthentication(EnableAuthenticatorViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Verify the code
        var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError("Code", "Verification code is invalid.");
            return View(model);
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        TempData["StatusMessage"] =
            "Your authenticator app has been verified and two-factor authentication has been enabled.";

        return RedirectToAction(nameof(ManageAccount));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableTwoFactorAuthentication()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            TempData["ErrorMessage"] = "Error disabling two-factor authentication.";
        else
            TempData["StatusMessage"] = "Two-factor authentication has been disabled.";

        return RedirectToAction(nameof(ManageAccount));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> PrivacySettings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // In a real application, you would load privacy settings from a separate table
        var model = new PrivacySettingsViewModel
        {
            ProfileVisibility = ProfileVisibility.Everyone,
            AllowFriendRequests = true,
            ShowOnlineStatus = true,
            AllowTagging = true
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PrivacySettings(PrivacySettingsViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // In a real application, you would save these settings to a separate table
        // For now, we'll just show a success message
        TempData["StatusMessage"] = "Your privacy settings have been updated.";
        return RedirectToAction(nameof(PrivacySettings));
    }

    [HttpGet]
    [Authorize]
    public IActionResult DeleteAccount()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDeleteAccount(DeleteAccountViewModel model)
    {
        if (!ModelState.IsValid) return View("DeleteAccount", model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!checkPassword)
        {
            ModelState.AddModelError("Password", "Incorrect password.");
            return View("DeleteAccount", model);
        }

        // In a real application, you might want to:
        // 1. Soft delete the user (mark as inactive)
        // 2. Schedule hard deletion for later
        // 3. Delete or anonymize user content
        // 4. Handle related data (friends, posts, etc.)

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded) throw new InvalidOperationException("Unexpected error occurred deleting user.");

        await _signInManager.SignOutAsync();

        return RedirectToAction("AccountDeleted");
    }

    [AllowAnonymous]
    public IActionResult AccountDeleted()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendVerificationEmail()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var userId = await _userManager.GetUserIdAsync(user);
        var email = await _userManager.GetEmailAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // In a real application, you would send an email with the confirmation link
        var callbackUrl = Url.Action("ConfirmEmail", "Account",
            new { userId = userId, code = code }, protocol: Request.Scheme);

        await _emailService.SendEmailAsync(user.Email, "Confirm email", callbackUrl);
        TempData["StatusMessage"] = "Verification email sent. Please check your email.";
        return RedirectToAction(nameof(ManageAccount));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ChangeEmail()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new ChangeEmailViewModel
        {
            Email = user.Email
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var email = await _userManager.GetEmailAsync(user);
        if (model.NewEmail != email)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);

            // In a real application, you would send an email with the confirmation link
            // var callbackUrl = Url.Action("ConfirmEmailChange", "Account", 
            //     new { userId = userId, email = model.NewEmail, code = code }, protocol: Request.Scheme);

            TempData["StatusMessage"] = "Confirmation link sent. Please check your email.";
            return RedirectToAction(nameof(ManageAccount));
        }

        TempData["StatusMessage"] = "Your email is unchanged.";
        return RedirectToAction(nameof(ManageAccount));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string code)
    {
        if (userId == null || email == null || code == null) return RedirectToAction("Index", "Home");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");

        var result = await _userManager.ChangeEmailAsync(user, email, code);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Error changing email.";
            return RedirectToAction(nameof(ManageAccount));
        }

        // Update username to match new email
        await _userManager.SetUserNameAsync(user, email);

        await _signInManager.RefreshSignInAsync(user);
        TempData["StatusMessage"] = "Your email has been changed.";
        return RedirectToAction(nameof(ManageAccount));
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        return string.Format(
            AuthenticatorUriFormat,
            Uri.EscapeDataString("SocialMediaApp"),
            Uri.EscapeDataString(email),
            unformattedKey);
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}