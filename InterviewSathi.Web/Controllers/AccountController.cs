using InterviewSathi.Web.Models.Entities;
using InterviewSathi.Web.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace InterviewSathi.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
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

        [HttpGet]
        public IActionResult Register()
        {
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
                    EmailConfirmed = true,
                    UserName = registerVM.Email,
                    ProfileURL = "ProfilePic.jpeg",
                    CoverURL = "coverpic.jpg",
                    CreatedAt = DateTime.UtcNow
                };

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

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    if (string.IsNullOrEmpty(registerVM.RedirectUrl))
                    {
                        return RedirectToAction("Index", "Blog");
                    }
                    else
                    {
                        return LocalRedirect(registerVM.RedirectUrl);
                    }
                }
            }
            return View(registerVM);
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
                        return RedirectToAction("Dashboard");
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
            returnurl = returnurl ?? Url.Content("~/");
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

            //sign in the user with this external login provider. only if they have a login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                               isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfVM model,
            string returnurl = null)
        {
            returnurl = returnurl ?? Url.Content("~/");

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
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);

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
