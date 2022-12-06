using MatrixIdent.Services;
using Microsoft.EntityFrameworkCore;
using MatrixIdent.Database;
using AspNetCoreRateLimit;
using log4net.Config;
using Utils.Other;

XmlConfigurator.Configure(new FileInfo("log4net.config"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();

ConfigService config = new ConfigService(builder.Configuration);

// Add services to the container.

builder.Services.AddSingleton<ConfigService>(o => config);

var optionsBuilder = new DbContextOptionsBuilder<IdentDbContext>();
optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));

builder.Services.AddSingleton(new DBService(config));
builder.Services.AddSingleton(new LDAPService(config));

builder.Services.AddControllers();

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

app.UseHttpsRedirection();
app.UseCors(policy => policy.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<CustomIpRateLimitMiddleware>();

app.Run();
