using HelthTrack.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
using HelthTrack.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews(options => {
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
    options.Filters.Add<HelthTrack.Filters.DatabaseExceptionFilter>();
});
var connection = builder.Configuration.GetConnectionString("HealthTrack");
if (string.IsNullOrWhiteSpace(connection))
    throw new InvalidOperationException("Configure ConnectionStrings:HealthTrack using user secrets or an environment variable.");
builder.Services.AddDbContext<DataBaseContext>(option => option.UseSqlServer(connection));
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) => {
        context.HttpContext.Response.Headers.RetryAfter = "300";
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("Too many account requests. Please wait five minutes and try again.", cancellationToken);
    };
    options.AddPolicy("account", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions {
            PermitLimit = 30, Window = TimeSpan.FromMinutes(5), QueueLimit = 0,
        }));
});

// Session configuration (أفضل من AddSession لحالها)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // مدة انتهاء الجلسة
    options.Cookie.HttpOnly = true;                 // حماية أفضل
    options.Cookie.IsEssential = true;              // مهم حتى يشتغل بدون موافقات كوكيز
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

if (builder.Configuration["maintenance"] is string maintenance)
{
    await AccountMaintenance.RunAsync(maintenance, app.Services, builder.Configuration);
    return;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

// تفعيل الـ Session (لازم يكون قبل أي شي يعتمد على السشن)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.Run();
