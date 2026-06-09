using Application.Services;
using Application.Interfaces;
using Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestsApi.Data;
using QuestsApi.Hubs;
using QuestsApi.Middleware;
using QuestsApi.Services;
using System.Text;

namespace QuestsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            // SignalR должен быть зарегистрирован ДО любых сервисов, использующих IHubContext
            builder.Services.AddSignalR();
            builder.Services.AddScoped<QuestRepository>();
            builder.Services.AddScoped<QuestService>();
            builder.Services.AddScoped<AuthRepository>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<QuestSessionService>();
            builder.Services.AddScoped<CategoryService>();
            // Реализация нотификаций через SignalR (зависит от IHubContext, который теперь доступен)
            builder.Services.AddScoped<IQuestNotifier, QuestHubNotifier>();
            builder.Services.AddScoped<QuestGeneratorService>();
            builder.Services.AddScoped<RegistrationTokenRepository>();
            builder.Services.AddScoped<TemplateRenameRepository>();

            builder.Services.AddHostedService<RegistrationTokenCleanupService>();
builder.Services.AddScoped<IImageService, CloudinaryImageService>();

            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<QuestPlatformContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("QuestPlatform")));

            var jwtKey = builder.Configuration["Jwt:Key"]
                         ?? throw new InvalidOperationException("JWT Key is not configured");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

            var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "http://localhost:3000,https://localhost:3000")
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy => policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            builder.Services.AddSwaggerGen();

            // Явно задаём URL для development (совместимость с фронтендом)
            // В production Docker переопределяет через ASPNETCORE_URLS
            if (builder.Environment.IsDevelopment())
            {
                builder.WebHost.UseUrls("http://localhost:7240");
            }

            var app = builder.Build();

            // Применяем миграции для обновления схемы БД
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<QuestPlatformContext>();
                context.Database.Migrate();
            }

            using (var scope = app.Services.CreateScope())
            {
                var categoryService = scope.ServiceProvider.GetRequiredService<CategoryService>();
                categoryService.SeedDefaultCategoriesAsync().GetAwaiter().GetResult();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ExceptionMiddleware>();

            app.MapControllers();
            app.MapHub<QuestHub>("/questHub");

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Run();
        }
    }
}
