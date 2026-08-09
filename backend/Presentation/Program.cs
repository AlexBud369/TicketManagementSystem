using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
        Title = "Ticket Management System API",
        Version = "v1",
        Description = "REST API for the Ticket Management System"
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
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new {
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

app.Run();
