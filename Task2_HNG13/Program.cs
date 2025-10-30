
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System;
using Task2_HNG13.Data;
using Task2_HNG13.Filters;
using Task2_HNG13.Repositories;

namespace Task2_HNG13
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<AppDbContext>(option =>
           option.UseNpgsql(builder.Configuration.GetConnectionString("contextConnection")));
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            builder.WebHost.UseUrls($"http://*:{port}");
            builder.Services.AddSingleton<Kernel>(serviceProvider =>
            {
                var apiKey = builder.Configuration["Groq:ApiKey"]
                           ?? throw new InvalidOperationException("Groq API Key not configured");

                var modelId = builder.Configuration["Groq:ModelId"] ?? "mixtral-8x7b-32768";
                var baseUrl = builder.Configuration["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1/";

                var kernelBuilder = Kernel.CreateBuilder();

                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: modelId,
                    apiKey: apiKey,
                    httpClient: new HttpClient { BaseAddress = new Uri(baseUrl) });

                return kernelBuilder.Build();
            });
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<SqlQueryGenerator>();
            builder.Services.AddScoped<CountryRepository>();
            //option.UseNpgsql(connectionString));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            //app.Run();

            app.Run($"http://0.0.0.0:{port}");
        }
    }
}
