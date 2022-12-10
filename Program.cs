using MatrixIdent.Services;
using Microsoft.EntityFrameworkCore;
using MatrixIdent.Database;
using AspNetCoreRateLimit;
using log4net.Config;
using Utils.Other;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

var securityScheme = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web Token based security",
};

var securityReq = new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }
};

var contactInfo = new OpenApiContact()
{
    Name = config.get("Info:name"),
    Email = config.get("Info:mail"),
    Url = new Uri(config.get("Info:url") != "" ? config.get("Info:url") : "http://localhost")
};

var license = new OpenApiLicense()
{
    Name = "Free License",
};

var info = new OpenApiInfo()
{
    Version = config.getApiVersion(),
    Title = config.get("name") + " Documentation " + config.getApiVersion(),
    Contact = contactInfo,
    License = license
};

builder.Services.AddControllers().AddXmlSerializerFormatters();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", info);
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityReq);
});

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["auth:Jwt:Issuer"],
        ValidAudience = builder.Configuration["auth:Jwt:Audience"],
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["auth:Jwt:Key"])),
        ValidateLifetime = true, // In any other application other then demo this needs to be true,
        ValidateIssuerSigningKey = false
    };
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(policy => policy.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<CustomIpRateLimitMiddleware>();

app.Run();
