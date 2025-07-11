using System.Text;
using System.Threading.RateLimiting;
using CollageManagementSystem.Core;
using CollageManagementSystem.Core.Entities.userEnrollments;
using CollageManagementSystem.Services;
using CollageManagementSystem.Services.Auth;
using CollageMangmentSystem.Core.Interfaces;
using CollageMangmentSystem.Infrastructure.Data;
using CollageMangmentSystem.Infrastructure.Data.Repositories;
using CollageMangmentSystem.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Add infrastructure services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"),
        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    )
);

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IDepRepostaory<>), typeof(DepRepostaory<>));
builder.Services.AddScoped(typeof(ICourseReposatory<>), typeof(CourseReposatory<>));
builder.Services.AddScoped(typeof(IUserEnrollments<>), typeof(UserEnrollmentsRepostaory<>));
builder.Services.AddScoped<IAdminReposatory, AdminReposatory>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();

// Configure cookie policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always; // Force secure cookies
    options.HttpOnly = HttpOnlyPolicy.Always;
});

// Configure authentication cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "CollageAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.LoginPath = "/api/auth/login";
    options.AccessDeniedPath = "/api/auth/access-denied";
});

// Configure JWT cookie settings
builder.Services.Configure<CookieOptions>(options =>
{
    options.SameSite = SameSiteMode.None;
    options.Secure = true;
    options.HttpOnly = true;
    options.IsEssential = true;
});

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        "FixedWindowPolicy",
        opt =>
        {
            opt.Window = TimeSpan.FromSeconds(10);
            opt.PermitLimit = 5;
            opt.QueueLimit = 0;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        }
    );

    options.AddFixedWindowLimiter(
        "StrictPolicy",
        opt =>
        {
            opt.Window = TimeSpan.FromMinutes(10);
            opt.PermitLimit = 10;
            opt.QueueLimit = 0;
        }
    );

    options.OnRejected = (context, _) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = (
            600 - DateTime.Now.Second % 600
        ).ToString();
        return new ValueTask();
    };

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey =
    jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to get token from cookie first
                context.Token =
                    context.Request.Cookies["accessToken"]
                    ?? context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                return Task.CompletedTask;
            },
        };
    });

// Register application services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:3000", "https://campus-core.vercel.app")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials() // Important for cookies
                .SetPreflightMaxAge(TimeSpan.FromHours(1));
        }
    );
});

var app = builder.Build();
app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Use(
    async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        await next(context); // Proceed to the next middleware
    }
);

app.UseHttpsRedirection();

app.UseCookiePolicy(
    new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.None,
        Secure = CookieSecurePolicy.SameAsRequest,
        HttpOnly = HttpOnlyPolicy.Always,
    }
);

app.UseMiddleware<GlobalExceptionMiddleware>();

// Use middleware with factory approach
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
var port = Environment.GetEnvironmentVariable("PORT") ?? "5168";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
