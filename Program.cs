using MatrixIdent.Services;
using Microsoft.EntityFrameworkCore;
using MatrixIdent.Database;
using AspNetCoreRateLimit;
using MatrixIdent;

var builder = WebApplication.CreateBuilder(args);

ConfigService config = new ConfigService(builder.Configuration);

// Add services to the container.

builder.Services.AddSingleton<ConfigService>(o => config);

var optionsBuilder = new DbContextOptionsBuilder<IdentDbContext>();
optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));

builder.Services.AddSingleton(new DBService(config));
builder.Services.AddSingleton(new LDAPService(config));

builder.Services.AddControllers();

builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();

builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("ClientRateLimiting"));
builder.Services.Configure<ClientRateLimitPolicies>(builder.Configuration.GetSection("ClientRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, /*Custom*/RateLimitConfiguration>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseClientRateLimiting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
