using SocialNetwork.Data;

namespace SocialNetwork
{
    public class UserOnlineChecker
    {
        private ISocialRepo _repository;
        private  IServiceScope _scope;
        private readonly Thread _CheckUsersOnline;
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        public UserOnlineChecker(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;

            _serviceProvider = serviceProvider;

            _CheckUsersOnline = new Thread(() => CheckAllUsersOnline(60000, 3));
            _CheckUsersOnline.Start();
        }

        public void CheckAllUsersOnline(int checkingIntervalMs, int onlineTimeoutMinutes)
        {
            while (true)
            {
                _scope = _serviceProvider.CreateScope();
                _repository = _scope.ServiceProvider.GetService<ISocialRepo>();

                foreach (var item in _repository.GetAllUsers())
                {
                    if ((item.LastOnlineTime.AddMinutes(onlineTimeoutMinutes) < DateTime.Now) && item.IsOnline)
                    {
                        item.IsOnline = false;
                        _repository.SaveChanges();
                    }
                }
                Thread.Sleep(checkingIntervalMs);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next.Invoke(context);
        }
    }
}
