using Application;
using Infrastructure;
using Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;
using Presentation.Middleware;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        standardErrorFromLevel: LogEventLevel.Verbose,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try {
    Log.Information("Ticket API starting");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

const int defaultMaxImageSizeMb = 5;
var maxImageSizeMb = builder.Configuration.GetValue(
    "AppSettings:MaxImageSizeMb",
    defaultMaxImageSizeMb);
if (maxImageSizeMb <= 0) {
    maxImageSizeMb = defaultMaxImageSizeMb;
}

var maxRequestBodyBytes = maxImageSizeMb * 1024L * 1024L;

builder.WebHost.ConfigureKestrel(options => {
    options.Limits.MaxRequestBodySize = maxRequestBodyBytes;
});
builder.Services.Configure<IISServerOptions>(options => {
    options.MaxRequestBodySize = maxRequestBodyBytes;
});
builder.Services.Configure<FormOptions>(options => {
    options.MultipartBodyLengthLimit = maxRequestBodyBytes;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
        Title = "Ticket Management System API",
        Version = "v1",
        Description = "REST API for the Ticket Management System"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the access token. Do not include the 'Bearer ' prefix."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy
            .WithOrigins(
                builder.Configuration["AppSettings:FrontendUrl"] ?? "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

var isLocalEnvironment = app.Environment.IsDevelopment() ||
                         app.Environment.IsEnvironment("Docker");

if (isLocalEnvironment) {
    app.UseSwagger();
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticket Management API v1");
        options.RoutePrefix = "swagger";
    });
}

if (!isLocalEnvironment) {
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new {
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

await IdentitySeeder.SeedAsync(app.Services);
    Log.Information("Ticket API listening. Email links are written to this console (docker logs -f ticket-api).");

    await app.RunAsync();
}
catch (Exception exception) {
    Log.Fatal(exception, "Ticket API terminated unexpectedly");
    throw;
}
finally {
    await Log.CloseAndFlushAsync();
}
