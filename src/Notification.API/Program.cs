using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using Notification.Application;
using Notification.Application.Common.Interfaces;
using Notification.API.Hubs;
using Notification.API.RealTime;
using Notification.Infrastructure;
using Notification.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Layers
builder.Services.AddNotificationApplication();
builder.Services.AddNotificationInfrastructure(builder.Configuration);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DevRadar Notification Service API", Version = "v1" });
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

// SignalR + bridge notifier
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealTimeNotifier, SignalRNotifier>();

// CORS cho frontend dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins("http://localhost:3000", "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

// Request logging: đặt ĐẦU pipeline để log mọi request kèm thời gian phản hồi
app.UseMiddleware<Notification.API.Middleware.RequestLoggingMiddleware>();

app.UseMiddleware<Notification.API.Middleware.ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

// Auto-migrate database on startup (cần thiết khi chạy trong Docker container)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Notification.Infrastructure.Persistence.NotificationDbContext>();
    db.Database.Migrate();
}

app.Run();
