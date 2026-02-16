
using Microsoft.EntityFrameworkCore;
using QuestsApi.Data;
using Application.Services;
using Data.Repositories;

namespace QuestsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddScoped<QuestRepository>();
            builder.Services.AddScoped<QuestService>();
            builder.Services.AddScoped<AuthRepository>();
            builder.Services.AddScoped<AuthService>();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<QuestPlatformContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QuestPlatform")));

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

            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.MapOpenApi();
            //}

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseStaticFiles();

            app.UseCors("AllowNext");
            app.MapControllers();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Run();
        }
    }
}
