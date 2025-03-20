using Microsoft.AspNetCore.Identity;
using SocialMediaApp.Data;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services;

public class AccountService : IAccountService
{
    private readonly IWebHostEnvironment _environment;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public AccountService(IWebHostEnvironment environment, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _environment = environment;
        _signInManager = signInManager;
        _userManager = userManager;
        _emailService = emailService;
    }

    public Task<bool> LoginAsync(LoginViewModel model)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RegisterAsync(RegisterViewModel model)
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
        return result.Succeeded;
    }

    public Task<bool> Logout(RegisterViewModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Edit(EditProfileViewModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ForgotPassword(ForgotPasswordViewModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ResetPassword(ResetPasswordViewModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ConfirmEmail(string userId, string code)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EnableTwoFactorAuthentication(EnableAuthenticatorViewModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DisableTwoFactorAuthentication()
    {
        throw new NotImplementedException();
    }

    public Task<bool> ConfirmDeleteAccount(DeleteAccountViewModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendVerificationEmail()
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangeEmail(ChangeEmailViewModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ConfirmEmailChange(string userId, string email, string code)
    {
        throw new NotImplementedException();
    }

    public Task<bool> GetUserIsNotFriend()
    {
        throw new NotImplementedException();
    }
}