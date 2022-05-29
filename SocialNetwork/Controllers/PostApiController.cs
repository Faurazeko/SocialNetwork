using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Data;
using SocialNetwork.Dtos;
using SocialNetwork.Models;


namespace SocialNetwork.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostApiController : ControllerBase
    {
        private readonly ISocialRepo _repository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        private readonly string _fileStoragePath;

        public PostApiController(ISocialRepo repository, IMapper mapper, IWebHostEnvironment env)
        {
            _repository = repository;
            _mapper = mapper;
            _env = env;

            _fileStoragePath = $"{_env.WebRootPath}\\FileStorage";
        }

        [HttpGet("{postId}", Name = "GetPost")]
        public IActionResult GetPost(int postId)
        {
            var post = _repository.GetPost(postId);

            return Ok(_mapper.Map<PostReadDto>(post));
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult PublishPost(
            [FromForm(Name = "content")] string content, 
            [FromForm(Name = "files")] List<IFormFile> files, 
            [FromForm(Name = "browserRequest")] bool? browserRequest)
        {
            User user = _repository.GetUser(User.FindFirst("username").Value.ToLower());

            if (user == null)
                return NotFound();

            if (content == null && files.Count < 1)
                return BadRequest();

            content = content ?? "";

            var post = new Post() { Content = content, User = user, UserId = user.Id, CreatedDate = System.DateTime.Now };

            _repository.CreatePost(post);
            _repository.SaveChanges();

            var postDir = $"{_fileStoragePath}\\Posts\\{user.Nickname}\\{post.Id}";
            var internetPostDir = $"\\FileStorage\\Posts\\{user.Nickname}\\{post.Id}";

            if (files.Count > 0)
                Directory.CreateDirectory(postDir);

            var filesList = new string[files.Count];

            for (int i = 0; i < files.Count; i++)
            {
                using (var fs = new FileStream($"{postDir}\\{files[i].FileName}", FileMode.OpenOrCreate))
                {
                    files[i].CopyTo(fs);
                    filesList[i] = $"{internetPostDir}\\{files[i].FileName}";
                }
            }

            post.FilesDirs = filesList;
            _repository.SaveChanges();

            var postReadDto = _mapper.Map<PostReadDto>(post);

            if (browserRequest != null)
                if (browserRequest == true)
                    return Redirect($"\\u\\{user.Nickname}");


            return CreatedAtRoute(nameof(GetPost), new { postId = post.Id }, postReadDto);
        }

        [HttpDelete("{postId}")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult DeletePost(int postId)
        {
            Post post = _repository.GetPost(postId);
            var username = User.FindFirst("username").Value;
            var userId = Convert.ToInt32(User.FindFirst("userId").Value);

            if ((post == null) || (post.UserId != userId))
                return BadRequest();

            _repository.DeletePost(postId);
            _repository.SaveChanges();

            return Ok();
        }

        [HttpPut("{postId}/Emotion")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult SetEmotion(int postId, [FromForm] bool likeIt)
        {
            User user = _repository.GetUser(User.FindFirst("username").Value.ToLower());

            if (user == null)
                return StatusCode(403);

            if (likeIt == _repository.GetUserEmotion(user.Id, postId))
                _repository.PostRemoveEmotion(postId, user.Id);
            else
                _repository.PostSetEmotion(postId, user.Id, likeIt);

            _repository.SaveChanges();

            return Ok();
        }

        [HttpGet("{postId}/Emotion")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
            
        public IActionResult GetEmotion(int postId)
        {
            User user = _repository.GetUser(User.FindFirst("username").Value.ToLower());

            if (user == null)
                return StatusCode(403);

            return Ok(new { islike = _repository.GetUserEmotion(user.Id, postId) });
        }

        [HttpPut("{postId}")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult EditPost(
            int postId, 
            [FromForm] string contentNew, 
            [FromForm(Name = "filesToDelete")] List<string> filesToDelete, 
            [FromForm(Name = "filesToUpload")] List<IFormFile> filesToUpload)
        {
            Post post = _repository.GetPost(postId);
            var username = User.FindFirst("username").Value;
            var userId = Convert.ToInt32(User.FindFirst("userId").Value);

            if ((post == null) || (post.UserId != userId))
                return RedirectToAction("UserProfile", new { userNickName = username });

            post.Content = contentNew;
            post.IsEdited = true;
            post.EditedTime = DateTime.Now;

            _repository.SaveChanges();

            return Ok();
        }
    }
}
