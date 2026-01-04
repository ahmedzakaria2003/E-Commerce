using Identity.Infrastrcture;
using Identity.Api.Extensions;
using Identity.Application;

using Identity.Shared.Settigns;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
);


// Shared
builder.Services.AddSharedServices();

// Web Layer (Identity + DbContext + JWT)
builder.Services.AddWebServices(builder.Configuration);

// Authentication (JWT)
builder.Services.AddCustomAuthentication(builder.Configuration);

// Infrastructure
builder.Services.AddInfrastructureServices(builder.Configuration);

// Application
builder.Services.AddApplicationServices(builder.Configuration);


// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddCustomSwagger("Identity API","v1");

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseCustomSwagger("Identity API");

}

//  Request Logging Middleware (structured)
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : httpContext.Response.StatusCode > 499
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode > 399
                ? LogEventLevel.Warning
                : LogEventLevel.Information;

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value!);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault()!);
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString()!);
    };
});

// Static Files - Images folder for products
var imagesPath = Path.Combine(app.Environment.ContentRootPath, "images");
if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information(" Starting the API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, " Application start-up failed!");
}
finally
{
    Log.CloseAndFlush();
}