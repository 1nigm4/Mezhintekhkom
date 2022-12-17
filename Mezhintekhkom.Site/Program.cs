using Mezhintekhkom.Site.Data;
using Mezhintekhkom.Site.Data.Entities;
using Mezhintekhkom.Site.Properties;
using Mezhintekhkom.Site.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IEmailSender, MessageService>();
builder.Services.AddTransient<ISmsSender, MessageService>();
builder.Services.Configure<MessageServiceConfiguration>(builder.Configuration.GetSection("Services"));

ExternalConfiguration oAuth = new ExternalConfiguration();
builder.Configuration.GetSection("ExternalProviders").Bind(oAuth);

builder.Services
    .AddAuthentication()
    .AddVkontakte(options =>
    {
        options.ClientId = oAuth.Vkontakte.ClientId;
        options.ClientSecret = oAuth.Vkontakte.ClientSecret;
        options.Scope.AddRange(oAuth.Vkontakte.Scopes);
        options.Fields.AddRange(oAuth.Vkontakte.Fields);
        options.ClaimActions.MapJsonKey("image", "photo_100");
        options.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, "bdate");
        options.ClaimActions.MapJsonSubKey(ClaimTypes.MobilePhone, "contacts", "mobile_phone");
        options.ClaimActions.MapJsonKey(ClaimTypes.Gender, "sex");
    })
    .AddGoogle(options =>
    {
        options.ClientId = oAuth.Google.ClientId;
        options.ClientSecret = oAuth.Google.ClientSecret;
        options.Scope.AddRange(oAuth.Google.Scopes);
        options.ClaimActions.MapJsonKey("image", "picture");
        options.ClaimActions.MapJsonSubKey(ClaimTypes.DateOfBirth, "birthdays", "text");
        options.ClaimActions.MapJsonSubKey(ClaimTypes.MobilePhone, "phoneNumbers", "value");
        options.ClaimActions.MapJsonSubKey(ClaimTypes.Gender, "genders", "value");
    })
    .AddYandex(options =>
    {
        options.ClientId = oAuth.Yandex.ClientId;
        options.ClientSecret = oAuth.Yandex.ClientSecret;
        options.ClaimActions.MapJsonKey("image", "default_avatar_id");
        options.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, "birthday");
        options.ClaimActions.MapJsonSubKey(ClaimTypes.MobilePhone, "default_phone","number");
        options.ClaimActions.MapJsonKey(ClaimTypes.Gender, "sex");
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();