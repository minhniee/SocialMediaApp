using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services;

public interface IAccountService
{

	Task<bool> LoginAsync(LoginViewModel model);
	Task<bool> RegisterAsync(RegisterViewModel model);
	Task<bool> Logout(RegisterViewModel model);
	Task<bool> Edit(EditProfileViewModel model);
	Task<bool> ForgotPassword(ForgotPasswordViewModel model);
	Task<bool> ResetPassword(ResetPasswordViewModel model);
	Task<bool> ConfirmEmail(string userId, string code);
	Task<bool> EnableTwoFactorAuthentication(EnableAuthenticatorViewModel model);
	Task<bool> DisableTwoFactorAuthentication();
	Task<bool> ConfirmDeleteAccount(DeleteAccountViewModel model);
	Task<bool> SendVerificationEmail();
	Task<bool> ChangeEmail(ChangeEmailViewModel model);
	Task<bool> ConfirmEmailChange(string userId, string email, string code);
	Task<bool> GetUserIsNotFriend();

}