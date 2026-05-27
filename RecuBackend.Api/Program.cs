using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RecuBackend.Api.Data.AppDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("AppDb");
    options.UseNpgsql(connStr);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RecuBackend.Api.Data.AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/", () => Results.Ok(new { name = "RecuBackend", status = "ok" }))
    .WithName("Root");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health");

app.Run();
