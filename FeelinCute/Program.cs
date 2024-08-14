using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.AspNetCore.Identity;
using EmailService;
using Microsoft.EntityFrameworkCore;
using FeelinCute.Areas.Identity.Data;
using FeelinCute.Models;
using FeelinCute.Controllers;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AppDbContextConnection") ?? throw new InvalidOperationException("Connection string 'AppDbContextConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddSingleton(connectionString);
builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddControllersWithViews();
EmailConfiguraion emailConfig = builder.Configuration.GetSection("EmailConfiguraion").Get<EmailConfiguraion>();
AdminOptions adminEmail = builder.Configuration.GetSection("AdminOptions").Get<AdminOptions>();
SJAuthentication sjAuthentication = builder.Configuration.GetSection("SJAuthentication").Get<SJAuthentication>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddSingleton(adminEmail);
builder.Services.AddSingleton(sjAuthentication);
builder.Services.AddHttpClient<TokenServices>();
builder.Services.AddControllers();
builder.Services.AddSingleton<TokenService>(); // If your TokenService is Singleton
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddHostedService<ScheduledEmailService>();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMvc();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddAuthentication()
    //.AddFacebook(options =>
    //{
    //    IConfigurationSection facebookConfig = builder.Configuration.GetSection("Authentication:FaceBook");
    //    options.AppId = facebookConfig["AppId"];
    //    options.AppSecret = facebookConfig["AppSecret"];
    //    options.CallbackPath = "/signin-facebook";
    //})
   .AddGoogle(options =>
   {
       IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
       options.ClientId = googleAuthNSection["ClientId"];
       options.ClientSecret = googleAuthNSection["ClientSecret"];
       options.CallbackPath = "/signin-google";
   });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication(); ;
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "SuperAdmin","Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    string email = "bakhrievshahzod@gmail.com";
    string password = "FuckdaPolice2024@";
    if (await userManager.FindByEmailAsync(email)==null)
    {
        var user = new AppUser();
        user.UserName=email;
        user.Email = email;
        await userManager.CreateAsync(user,password);
        await userManager.AddToRoleAsync(user,"SuperAdmin");
    }
}

app.MapRazorPages();
app.Run();
