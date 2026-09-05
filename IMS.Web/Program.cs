using Microsoft.AspNetCore.HttpOverrides;
using StackExchange.Redis;
using IMS.Helpers.Options;
using IMS.Web.Extensions;
using IMS.Web.HostedServices;

var builder = WebApplication.CreateBuilder(args);

// Redis connection
var redisConnection = builder.Configuration["Redis:Connection"]?? throw new InvalidOperationException("Redis connection string is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Options binding
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));

// Data access layer
builder.Services.AddIMSDataAccess();

// Business/service layer
builder.Services.AddIMSBusinessServices();

// Keycloak auth
builder.Services.AddKeycloakAuth(builder.Configuration);
builder.Services.AddPermissionAuthorization();

// Startup sync hosted service
builder.Services.AddHostedService<PermissionSyncStartupService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

if (!app.Environment.IsDevelopment())
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
    name: "student_login_alias",
    pattern: "Student/Login",
    defaults: new { area = "StudentPortal", controller = "Auth", action = "Login" });

app.MapControllerRoute(
    name: "student_portal_area",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();