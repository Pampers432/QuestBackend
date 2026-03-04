using Application.Services;
using Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestsApi.Data;
using System.Text;

namespace QuestsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddScoped<QuestRepository>();
            builder.Services.AddScoped<QuestService>();
            builder.Services.AddScoped<AuthRepository>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<QuestSessionService>();
            builder.Services.AddScoped<CategoryService>();

            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<QuestPlatformContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("QuestPlatform")));

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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowNext",
                    policy => policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://localhost:3000"));
            });

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();

            app.UseCors("AllowNext");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Run();
        }
    }
}
