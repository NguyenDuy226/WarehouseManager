using Warehouse.Api.Middleware;
using Warehouse.Infrastructure.Services;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Warehouse.Infrastructure.Data;
using Warehouse.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using ExpenseTracker.API.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Warehouse.Infrastructure.Services.Warehouse;
using Warehouse.Application.Interfaces;
using Microsoft.AspNetCore.RateLimiting;


var builder = WebApplication.CreateBuilder(args);
//Logging
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;    
    options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions 
    { 
        Indented = true 
    };
});
//CORS
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
     options.AddPolicy(name: "local", 
                        policy =>
                        {
                            policy.WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                        });
});
//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Name = "Authorization"
    });
    options.AddSecurityRequirement(require => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", require)] = []
    });
});
builder.Services.AddControllers();
//infrastructure and health check 
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks()
                .AddDbContextCheck<WarehouseDbContext>();
//Identity
var identityConfig = builder.Configuration.GetSection("IdentitySettings");
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = identityConfig.GetValue<bool>("Password:RequireDigit");
    options.Password.RequireLowercase = identityConfig.GetValue<bool>("Password:RequireLowercase");
    options.Password.RequireUppercase = identityConfig.GetValue<bool>("Password:RequireUppercase");
    options.Password.RequireNonAlphanumeric = identityConfig.GetValue<bool>("Password:RequireNonAlphanumeric");
    options.Password.RequiredLength = identityConfig.GetValue<int>("Password:RequiredLength");
    var lockoutMinutes = identityConfig.GetValue<double>("Lockout:DefaultLockoutTimeSpanInMinutes");
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutMinutes);
    options.Lockout.MaxFailedAccessAttempts = identityConfig.GetValue<int>("Lockout:MaxFailedAccessAttempts");
    options.Lockout.AllowedForNewUsers = identityConfig.GetValue<bool>("Lockout:AllowedForNewUsers");
})
.AddEntityFrameworkStores<WarehouseDbContext>()
.AddDefaultTokenProviders();   
//add rate limit
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login_policy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });
});
// ... sau đó áp dụng vào Controller

//Role seeder
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddOpenApi();

//Authorization and authorization
builder.Services.AddScoped<CreateToken>();
var jwtSetting = builder.Configuration["JWT"];
var keyJson = builder.Configuration["JWT:Key"];
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyJson ?? string.Empty));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = key,
        ClockSkew = TimeSpan.Zero

    };
});
builder.Services.AddAuthorization();
//Add service
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//Seeding
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.Seeding();
}

app.UseCors("local");
app.UseHealthChecks("/health");
//Middleware
app.UseMiddleware<CorrelationIdMiddleware> ();
app.UseMiddleware<ExceptionHandlingMiddleware> ();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

