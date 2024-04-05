using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InterviewSathi.Web.Models.Entities.BlogsEntity;
using System.Reflection.Metadata;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using InterviewSathi.Web.Data;
using Microsoft.EntityFrameworkCore;
using InterviewSathi.Web.Services;


namespace InterviewSathi.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment env,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env; 
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl=null)
        {
            returnUrl ??= Url.Content("~/Blog/Index");
            ViewBag.ReturnUrl = returnUrl;
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
                _roleManager.CreateAsync(new IdentityRole("Interviewer")).Wait();
                _roleManager.CreateAsync(new IdentityRole("Interviewee")).Wait();
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["error"] = "Password Error";
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["success"] = "Password Changed!!";
            return RedirectToAction("Index", "Profile", new { id = user.Id });
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/blog");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        { 
            if (ModelState.IsValid)
            {
                ApplicationUser user = new()
                {
                    Name = registerVM.Name,
                    Email = registerVM.Email,
                    PhoneNumber = registerVM.PhoneNumber,
                    NormalizedEmail = registerVM.Email.ToUpper(),
                    EmailConfirmed = false,
                    UserName = registerVM.Email,
                    ProfileURL = "ProfilePic.jpeg",
                    CoverURL = "coverpic.jpg",
                    CreatedAt = DateTime.UtcNow
                };
                if (registerVM.DocUpload != null)
                {
                    string filename = Guid.NewGuid() + Path.GetExtension(registerVM.DocUpload.FileName);
                    string imgpath = Path.Combine(_env.WebRootPath, "Images/Documents/", filename);
                    using (FileStream streamread = new FileStream(imgpath, FileMode.Create))
                    {
                        registerVM.DocUpload.CopyTo(streamread);
                    }
                    user.DocURL = filename;
                }
                var result = await _userManager.CreateAsync(user, registerVM.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(registerVM.Role))
                    {
                        await _userManager.AddToRoleAsync(user, registerVM.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "Interviewee");
                    }

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Build the email confirmation link
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, Request.Scheme);

                    EmailService.SendMail(user.Email, "Email Confirmation", $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.");

                    TempData["error"] = "Verify Email!!";
                    return RedirectToAction("Login", "Account");
                  
                }
            }
            return View(registerVM);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["success"] = "Email Verified!!";
                return RedirectToAction("Index", "Blog");
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager
                    .PasswordSignInAsync(loginVM.Email, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);


                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(loginVM.Email);
                    if (user != null)
                    {
                        if (!user.EmailConfirmed)
                        {
                            ModelState.AddModelError("", "Please confirm your email before logging in.");
                            return View(loginVM); 
                        }

                        var role = await _userManager.GetRolesAsync(user);
                        var claims = new List<Claim>
                               {
                                   new(ClaimTypes.Name, user.Id),
                                   new("UserName", user.Name),
                                   new(ClaimTypes.Role, role.FirstOrDefault())
                               };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        TempData["success"] = "Log in successful";

                        // Sign in the user and issue the authentication cookie
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    }
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(loginVM.RedirectUrl))
                        {
                            return RedirectToAction("Index", "Blog");
                        }
                        else
                        {
                            return LocalRedirect(loginVM.RedirectUrl);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }

            return View(loginVM);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginAsync(string provider, string returnurl = null)
        {
            returnurl = returnurl ?? Url.Content("~/Blog/Index");
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnurl });
            var properties =  _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnurl = null, string remoteError = null)
        {
            returnurl = returnurl ?? Url.Content("~/Blog/Index");
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Add the roles as claims
                var claims = new List<Claim>();
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Add these claims to the user's principal
                var identity = (ClaimsIdentity)info.Principal.Identity;
                identity.AddClaims(claims);
            }

                //sign in the user with this external login provider. only if they have a login
                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                               isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {

                TempData["success"] = "Log in successful";
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                if (IsUserAdmin(info.Principal))
                {
                    // Redirect admin to the dashboard
                    return RedirectToAction("Index", "Dashboard");
                }
                return LocalRedirect(returnurl);
            }
            else
            {
                //that means user account is not create and we will display a view to create an account

                ViewData["ReturnUrl"] = returnurl;
                ViewData["ProviderDisplayName"] = info.ProviderDisplayName;

                return View("ExternalLoginConfirmation", new ExternalLoginConfVM
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                    Name = info.Principal.FindFirstValue(ClaimTypes.Name),
                    Role = info.Principal.FindFirstValue(ClaimTypes.Role)
                });
            }
        }

        private bool IsUserAdmin(ClaimsPrincipal principal)
        {
            // Check if the principal contains a claim with the role "Admin"
            return principal.IsInRole("Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfVM model,
            string returnurl = null)
        {
            returnurl = returnurl ?? Url.Content("~/Blog/Index");

            if (ModelState.IsValid)
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("Error");
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    NormalizedEmail = model.Email.ToUpper(),
                    ProfileURL = "ProfilePic.jpeg",
                    CoverURL = "coverpic.jpg",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                if (model.DocUpload != null)
                {
                    string filename = Guid.NewGuid() + Path.GetExtension(model.DocUpload.FileName);
                    string imgpath = Path.Combine(_env.WebRootPath, "Images/Documents/", filename);
                    using (FileStream streamread = new FileStream(imgpath, FileMode.Create))
                    {
                        model.DocUpload.CopyTo(streamread);
                    }
                    user.DocURL = filename;
                }

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    var roleClaim = new Claim(ClaimTypes.Role, model.Role);
                    await _userManager.AddClaimAsync(user, roleClaim);

                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                        return LocalRedirect(returnurl);

                    }
                }
                AddErrors(result); 
                
            }
            ViewData["ReturnUrl"] = returnurl;
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
