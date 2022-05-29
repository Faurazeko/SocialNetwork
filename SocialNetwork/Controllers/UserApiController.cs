using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Data;
using SocialNetwork.Dtos;
using SocialNetwork.Models;
using System.Globalization;
using System.Security.Claims;

namespace SocialNetwork.Controllers
{
    [Route("api/user")]
    [Route("api/u")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly ISocialRepo _repository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        private readonly string _fileStoragePath;

        public UserApiController(ISocialRepo repository, IMapper mapper, IWebHostEnvironment env)
        {
            _repository = repository;
            _mapper = mapper;
            _env = env;

            _fileStoragePath = $"{_env.WebRootPath}\\FileStorage";
        }

        [HttpPut("status")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult SetStatus(string newStatus)
        {
            User user = _repository.GetUser(User.FindFirst("username").Value.ToLower());

            if (user == null)
                return StatusCode(403);

            if (newStatus == null)
                return BadRequest();

            user.AboutText = newStatus;
            _repository.SaveChanges();

            return Ok();
        }



        [HttpDelete("Avatar")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult DeleteAvatar()
        {
            var username = User.FindFirst("username").Value;

            var usrDir = $"{_fileStoragePath}\\Users\\{username}";
            var filePath = $"{usrDir}\\avatar.png";

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);

                var user = _repository.GetUser(username);
                user.AvatarPath = "/FileStorage/Default/avatar.png";
                _repository.SaveChanges();

                return Ok();
            }

            return NotFound();
        }

        [HttpPost("Avatar")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [RequestSizeLimit(5500000)]
        public IActionResult UploadAvatar(IFormFile file)
        {
            if (file == null)
                return BadRequest();

            var fileExt = file.FileName.Split('.').Last().ToLower();
            string[] allowedExtensions = new[] { "png", "jpg", "jpeg", "gif" };
            var isExtAllowed = false;

            foreach (string extension in allowedExtensions)
            {
                if (extension == fileExt)
                {
                    isExtAllowed = true;
                    break;
                }
            }

            if (!isExtAllowed)
                return BadRequest();

            var username = User.FindFirst("username").Value;

            var usrDir = $"{_fileStoragePath}\\Users\\{username}";
            var filePath = $"{usrDir}\\avatar.png";

            if (!Directory.Exists(usrDir))
                Directory.CreateDirectory(usrDir);

            using (Stream fs = new FileStream(filePath, FileMode.OpenOrCreate))
                file.CopyTo(fs);

            var internetAvatarPath = $"\\FileStorage\\Users\\{username}\\avatar.png";

            var user = _repository.GetUser(username);
            user.AvatarPath = internetAvatarPath;
            _repository.SaveChanges();

            return Created(internetAvatarPath, file);
        }

        [HttpPut("Online")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult UpdateOnline()
        {
            User user = _repository.GetUser(User.FindFirst("username").Value.ToLower());

            if (user == null)
                return StatusCode(403);

            user.IsOnline = true;
            user.LastOnlineTime = DateTime.Now;

            _repository.SaveChanges();

            return Ok();
        }



        [HttpGet("{username}/Online")]
        [HttpGet("id/{userId}/Online")]
        public bool GetOnline(string username, int userId)
        {
            User user = GetUser(username, userId);

            if (user == null)
                return false;

            return user.IsOnline;
        }

        [HttpGet("{username}/LastOnline")]
        [HttpGet("id/{userId}/LastOnline")]
        public string GetLastOnline(string username, int userId)
        {
            User user = GetUser(username, userId);

            if (user == null)
                return "";

            return user.LastOnlineTime.ToString("dd MMMM yyyy (HH:mm)", CultureInfo.GetCultureInfo("en-US"));
        }



        [HttpDelete("{username}")]
        [HttpDelete("id/{userId}")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult DeleteUser(string username, int userId)
        {

            var claimsUserId = Convert.ToInt32(User.FindFirst("userId").Value.ToLower());
            var claimsUsername = User.FindFirst("username").Value.ToLower();

            User user = GetUser(username, userId);

            if(user == null)
                return BadRequest();

            var role = User.FindFirst(ClaimTypes.Role).Value.ToString().ToLower();
            var usernameMatch = username.ToLower() == claimsUsername;
            var userIdMatch = userId == claimsUserId;

            if((role == "admin") || usernameMatch || userIdMatch)
            {
                if(userId != 0)
                    _repository.DeleteUser(userId);
                else
                    _repository.DeleteUser(username);

                _repository.SaveChanges();

                return Ok();
            }

            return StatusCode(403);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody]UserCreateDto userDto)
        {
            var user = _mapper.Map<User>(userDto);

            var userWithSameUsername = _repository.GetUser(user.Nickname);

            if(userWithSameUsername != null)
                return BadRequest("User with the same username already exists!");

            user.CreatedDateTime = DateTime.Now;
            user.LastOnlineTime = DateTime.Now;
            user.Trollars = 0;

            _repository.CreateUser(user);
            _repository.SaveChanges();
            return Created($"\\user\\{user.Nickname}", _mapper.Map<UserReadDto>(user));
        }

        [HttpPut("Password")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult ChangePassword(string username, string password)
        {
            var isAdmin = User.FindFirst(ClaimTypes.Role).Value.ToString().ToLower() == "admin";
            var claimsUsername = User.FindFirst("username").Value.ToLower();

            User user = _repository.GetUser(username);

            if (user == null)
                return NotFound();

            if (isAdmin || claimsUsername == username)
            {
                user.Password = password;
                user.ForcedLogOutTime = DateTime.Now;
                _repository.SaveChanges();
            }

            return Ok();
        }

        [HttpGet("ForceLogout")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult ForceLogout()
        {
            User user = GetUser(User.FindFirst("username").Value.ToLower());

            if (user == null)
                return NotFound();

            user.ForcedLogOutTime = DateTime.Now;
            _repository.SaveChanges();

            return Ok();
        }

        [HttpPost("{username}/Friend")]
        [HttpPost("id/{userId}/Friend")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult AddAsFriend(string username, int userId)
        {
            User user = GetUser(username, userId);

            if (user == null)
                return NotFound();

            var userRequesterId = Convert.ToInt32(User.FindFirst("userId").Value.ToLower());

            _repository.AddAsFriend(userRequesterId, user.Id);
            _repository.SaveChanges();

            //notification for user

            return Ok();
        }

        [HttpDelete("{username}/Friend")]
        [HttpDelete("id/{userId}/Friend")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult RemoveAsFriend(string username, int userId)
        {
            User user = GetUser(username, userId);

            if(user == null)
                return NotFound();

            var userRequesterId = Convert.ToInt32(User.FindFirst("userId").Value.ToLower());

            var relation = _repository.GetFriendRelation(user.Id, userRequesterId);

            if (relation == null)
                return BadRequest();

            var approved = relation.IsApproved;

            _repository.RemoveAsFriend(user.Id, userRequesterId);
            _repository.SaveChanges();


            Console.WriteLine(_repository.GetFriendRelation(user.Id, userRequesterId) == null);
            Console.WriteLine(_repository.GetFriendRelation(userRequesterId, user.Id) == null);

            //if (approved)
            //{
            //    _repository.AddAsFriend(user.Id, userRequesterId, true);
            //    _repository.SaveChanges();
            //}

            return Ok();
        }

        [HttpPut("{username}/Friend/Ignore")]
        [HttpPut("id/{userId}/Friend/Ignore")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult IgnoreFriend(string username, int userId)
        {
            User user = GetUser(username, userId);

            if (user == null)
                return NotFound();

            var userRequesterId = Convert.ToInt32(User.FindFirst("userId").Value.ToLower());

            var relation = _repository.GetFriendRelation(user.Id, userRequesterId);

            if (relation == null)
                return BadRequest();

            relation.IsIgnored = true;
            _repository.SaveChanges();

            return Ok();
        }

        [HttpPut("{username}/Friends/Hide")]
        [HttpPut("id/{userId}/Friends/Hide")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult ChangeFriendsVisibility(string username, int userId, [FromForm] bool hide)
        {
            User user = GetUser(username, userId);

            if (user == null)
                return NotFound();

            user.IsFriendsHidden = hide;

            _repository.SaveChanges();

            return Ok();
        }


        [HttpGet("{username}")]
        public IActionResult CheckUsernameAvailability(string username)
        {
            var isAvailable = _repository.CheckUsernameAvailability(username);

            return new JsonResult(new { available = isAvailable });
        }

        [NonAction]
        public User GetUser(string username, int userId = 0)
        {
            User user = null;

            if (username == null)
                user = _repository.GetUser(userId);
            if (userId == 0)
                user = _repository.GetUser(username);

            return user;
        }
    }
}
