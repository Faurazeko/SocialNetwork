using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SocialNetwork;
using SocialNetwork.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<ISocialRepo, SocialRepo>();
builder.Services.AddHttpContextAccessor();

Console.WriteLine("--> using InMem Db");
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseInMemoryDatabase("InMem");
}, ServiceLifetime.Scoped);

//Console.WriteLine("--> using SqlServer Db");
//builder.Services.AddDbContext<AppDbContext>(opt =>
//{
//    opt.UseSqlServer(configuration.GetConnectionString("DbConnection"));
//});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/home/login";
    options.AccessDeniedPath = "/home/denied";
    options.Events = new CookieAuthenticationEvents()
    {
        //when trying to sign in
        OnSigningIn = async context =>
        {
            //here i can add some claims
            await Task.CompletedTask;
        },
        //when password and username is correct
        OnSignedIn = async context =>
        {
            await Task.CompletedTask;
        },
        //called on every auth request (every time that someone authentificated trying access some page that needs authentification)
        OnValidatePrincipal = async context =>
        {
            await Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserOnlineChecker>();
app.UseMiddleware<UserSecurityMiddleware>();
app.UseMiddleware<OnGetEvents>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

PrepDb.PrepPopulation(app, !app.Environment.IsDevelopment());

app.Run();
