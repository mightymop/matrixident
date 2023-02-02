using MatrixIdent.Services;
using log4net.Config;
using Utils.Other;
using Microsoft.IdentityModel.Logging;

XmlConfigurator.Configure(new FileInfo("log4net.config"));

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager cfgmgr = builder.Configuration;
ConfigService config = new ConfigService(cfgmgr);
IdentityModelEventSource.ShowPII = true;

builder.Services.AddSingleton(new DBService(config));
builder.Services.AddSingleton(new LDAPService(config));

SwaggerExtensions.ConfigureSwaggerBuilder(builder, cfgmgr);
ConfigurationHelper.configureBuilder(builder, cfgmgr, config);

var app = builder.Build();

SwaggerExtensions.ConfigureSwaggerApp(app, cfgmgr);
ConfigurationHelper.configureApp(app);

app.Run();




