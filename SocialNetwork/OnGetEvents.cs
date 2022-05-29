using SocialNetwork.Data;

namespace SocialNetwork
{
    public class OnGetEvents
    {
        private ISocialRepo _repository;
        private IServiceScope _scope;
        private IWebHostEnvironment _env;
        private readonly Thread _CheckUsersOnline;
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _fileStoragePath;

        public OnGetEvents(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;

            _serviceProvider = serviceProvider;

            _scope = _serviceProvider.CreateScope();
            _env = _scope.ServiceProvider.GetService<IWebHostEnvironment>();

            _fileStoragePath = $"{_env.WebRootPath}\\FileStorage";
        }

        public void SetAvatarPath(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
                return;

            var username = context.User.Claims.FirstOrDefault(c => c.Type == "username").Value;

            var avatarPath = $"{_fileStoragePath}\\Users\\{username}\\avatar.png";

            if (File.Exists(avatarPath))
                avatarPath = $"/FileStorage/Users/{username}/avatar.png";
            else
                avatarPath = $"/FileStorage/Default/avatar.png";

            context.Items["AvatarPath"] = avatarPath;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _scope = _serviceProvider.CreateScope();
            _env = _scope.ServiceProvider.GetService<IWebHostEnvironment>();

            SetAvatarPath(context);

            await _next.Invoke(context);
        }
    }
}
