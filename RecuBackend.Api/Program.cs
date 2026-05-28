using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Data;
using RecuBackend.Api.Repositories.Ef;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services;
using RecuBackend.Api.Services.Dnd5e;
using RecuBackend.Api.Services.Domain.Impl;
using RecuBackend.Api.Services.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(_ => true));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Configuración JWT no encontrada.");
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("AppDb");
    options.UseNpgsql(connStr);
});

builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection(FileStorageSettings.SectionName));
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

builder.Services.Configure<Dnd5eApiSettings>(builder.Configuration.GetSection(Dnd5eApiSettings.SectionName));
builder.Services.AddHttpClient<IDnd5eApiClient, Dnd5eApiClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<Dnd5eApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddScoped<IUserContext, UserContextAccessor>();
builder.Services.AddSingleton<DiceRollerService>();
builder.Services.AddSingleton<TokenService>();

// Repositories (EF Core)
builder.Services.AddScoped<ICampaignRepository, EfCampaignRepository>();
builder.Services.AddScoped<ICharacterRepository, EfCharacterRepository>();
builder.Services.AddScoped<IRollRepository, EfRollRepository>();
builder.Services.AddScoped<IAttachmentRepository, EfAttachmentRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();

// Domain services
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IRollService, RollService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IAttachmentDownloadService, AttachmentDownloadService>();
builder.Services.AddScoped<IPublicService, PublicService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);

    var storage = scope.ServiceProvider.GetRequiredService<IOptions<FileStorageSettings>>().Value;
    Directory.CreateDirectory(storage.RootPath);
}

app.MapControllers();

app.MapGet("/", () => Results.Ok(new { name = "RecuBackend", status = "ok" }))
    .WithName("Root");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health");

app.Run();
