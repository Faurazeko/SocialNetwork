using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Data;
using SocialNetwork.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace SocialNetwork.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISocialRepo _userRepo;
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;
        private readonly string _fileStoragePath;

        public HomeController(ILogger<HomeController> logger, ISocialRepo userRepo, AppDbContext dbContext, IWebHostEnvironment env)
        {
            _logger = logger;
            _userRepo = userRepo;
            _dbContext = dbContext;
            _env = env;
            _fileStoragePath = $"{_env.WebRootPath}\\FileStorage";
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        public IActionResult Denied()
        {
            return View();
        }

        [NonAction]
        private ClaimsPrincipal generateClaimsPrincipal(User user)
        {
            string avatarPath = $"{_fileStoragePath}\\Users\\{user.Nickname}\\avatar.png";

            if (System.IO.File.Exists(avatarPath))
                avatarPath = $"/FileStorage/Users/{user.Nickname}/avatar.png";
            else
                avatarPath = $"/FileStorage/Default/avatar.png";

            var claims = new List<Claim>()
                        {
                            new Claim("username", user.Nickname),
                            new Claim("userId", user.Id.ToString()),
                            new Claim(ClaimTypes.NameIdentifier, user.Nickname),
                            new Claim(ClaimTypes.Name, user.Nickname),
                            new Claim("avatarPath", avatarPath),
                            new Claim("logInTime", DateTime.Now.ToString()),
                        };

            if (user.IsAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            else
                claims.Add(new Claim(ClaimTypes.Role, "User"));


            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimsPrincipal;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl)
        {
            foreach (var item in _dbContext.Users)
            {
                if (item.Nickname == username && item.Password == password)
                {
                    if (string.IsNullOrEmpty(returnUrl))
                        returnUrl = "/";

                    var claims = generateClaimsPrincipal(item);
                    await HttpContext.SignInAsync(claims);

                    return Redirect(returnUrl);
                }
            }

            TempData["Error"] = "Error. Username or password is incorrect. :(";
            return Redirect("login");
        }

        [HttpGet]
        [Route("/[controller]/Logout")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/[controller]/Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            if(statusCode == 404)
            {
                return View("HandleError/404");
            }
            else
            {
                return NotFound();
            }
        }
    }
}