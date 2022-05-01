using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Data;
using SocialNetwork.Models;
using System.Globalization;

namespace SocialNetwork.Controllers
{
    [Route("u")]
    [Route("user")]
    public class UserController : Controller
    {
        private readonly ISocialRepo _repository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        private readonly string _fileStoragePath;

        public UserController(ISocialRepo repository, IMapper mapper, IWebHostEnvironment env)
        {
            _repository = repository;
            _mapper = mapper;
            _env = env;

            _fileStoragePath = $"{_env.WebRootPath}\\FileStorage";
        } 

        [HttpGet("{userNickName}")]
        [HttpGet("id/{userId}")]
        public IActionResult UserProfile(int userId, string userNickName)
        {
            User user = null;

            if(string.IsNullOrEmpty(userNickName))
                user = _repository.GetUser(userId);
            else
                user = _repository.GetUser(userNickName);

            if(userNickName == "id0")
            {
                if (User.Identity.IsAuthenticated)
                    return Redirect($"/u/{User.FindFirst("username").Value}");
                else
                    return Redirect("/home/login?ReturnUrl=%2Fu%2id0");
            }

            if(user == null)
                return NotFound();

            string avatarPath = $"{_fileStoragePath}\\Users\\{user.Nickname}\\avatar.png";

            if (System.IO.File.Exists(avatarPath))
                ViewData["AvatarPath"] = $"/FileStorage/Users/{user.Nickname}/avatar.png";
            else
                ViewData["AvatarPath"] = $"/FileStorage/Default/avatar.png";

            return View("Profile", user);
        }

        [HttpGet("settings")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult SettingsPage()
        {
            User user = _repository.GetUser(User.FindFirst("username").Value.ToLower());

            return View("Settings", user);
        }
    }
}
