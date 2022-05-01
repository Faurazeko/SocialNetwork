using SocialNetwork.Data;

namespace SocialNetwork
{
    public class UserSecurityMiddleware
    {
        private ISocialRepo _repository;
        private IServiceScope _scope;
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        public UserSecurityMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;

            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _scope = _serviceProvider.CreateScope();
            _repository = _scope.ServiceProvider.GetService<ISocialRepo>();

            if (context.User.Identity.IsAuthenticated)
            {
                var loginTime = Convert.ToDateTime(context.User.FindFirst("logInTime").Value);
                var username = context.User.FindFirst("username").Value.ToLower();

                if (loginTime < _repository.GetUser(username).ForcedLogOutTime)
                {
                    context.Response.Redirect("/home/logout");
                }
            }


            await _next.Invoke(context);
        }
    }
}
