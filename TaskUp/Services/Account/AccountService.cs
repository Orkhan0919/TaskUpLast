using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using TaskUp.Models;
using TaskUp.ViewModels.Identity;
using TaskUp.Services.Email;

namespace TaskUp.Services.Account;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IEmailService emailService,
        ILogger<AccountService> logger)  
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _logger = logger; 
    }

    public async Task<IdentityResult> RegisterAsync(RegisterVM model)
    {
        var user = new AppUser
        {
            UserName = model.Username,
            Email = model.Email,
            Name = model.Name,
            Surname = model.Surname,
            DisplayName = $"{model.Name} {model.Surname}",
            AvatarUrl = "",
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendConfirmationEmailAsync(user.Email, token, user.DisplayName);
        }

        return result;
    }

    public async Task<SignInResult> LoginAsync(LoginVM login)
    {
        var user = await _userManager.FindByEmailAsync(login.Email);

        if (user == null)
        {
            return SignInResult.Failed;
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return SignInResult.NotAllowed;
        }

        return await _signInManager.PasswordSignInAsync(
            user.UserName,
            login.Password,
            login.RememberMe ?? false,
            lockoutOnFailure: true);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<IdentityResult> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User not found"
            });
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            await _emailService.SendWelcomeEmailAsync(user.Email, user.DisplayName);
        }

        return result;
    }

    public async Task<bool> IsEmailConfirmedAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;

        return await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger?.LogWarning("ForgotPassword called with empty email");
                return;
            }

            var user = await _userManager.FindByEmailAsync(email);
        
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
                var encodedToken = System.Net.WebUtility.UrlEncode(token);
            
                await _emailService.SendPasswordResetEmailAsync(
                    user.Email, 
                    encodedToken, 
                    user.DisplayName ?? user.UserName
                );
            
                _logger?.LogInformation($"Password reset email sent to: {email}");
            }
            else
            {
                _logger?.LogWarning($"Password reset attempted for non-existent or unconfirmed email: {email}");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error in ForgotPasswordAsync for email: {email}");
            throw;
        }
    }
    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordVM model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User not found"
            });
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        
        if (result.Succeeded)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }

        return result;
    }

    public async Task ResendConfirmationEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendConfirmationEmailAsync(user.Email, token, user.DisplayName);
        }
    }

    public async Task<AppUser> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<AppUser> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<bool> CheckPasswordAsync(AppUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
    {
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<IdentityResult> UpdateProfileAsync(AppUser user)
    {
        return await _userManager.UpdateAsync(user);
    }
}