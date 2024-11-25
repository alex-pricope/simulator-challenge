using System.Text;
using Gamebasics.Api.Middleware;
using Gamebasics.Domain.Interfaces;
using Gamebasics.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Gamebasics.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // Instantiate logger
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Filter.ByExcluding(@"RequestPath like '%/swagger%' 
                                or RequestPath like '%/_vs%'
                                or RequestPath like '%/_framework%'")
            .WriteTo.Console()
            .CreateLogger();
        Log.Logger = logger;

        var builder = WebApplication.CreateBuilder(args);

        // Register Serilog logger
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);
        builder.Services.AddLogging();

        // Add controller
        builder.Services.AddControllers();

        // Add the rest of the services
        builder.Services.AddTransient<ErrorHandlingMiddleware>();
        builder.Services.AddScoped<ISimulationService, SoccerSimulationService>();

        // Custom ErrorDetails for ValidationResult error
        // https://stackoverflow.com/questions/51145243/how-do-i-customize-asp-net-core-model-binding-errors
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                // Gather the errors from the model state and create the custom ErrorDetails
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value.Errors.Select(e => new { Field = kvp.Key, Message = e.ErrorMessage }));

                var stringBuilder = new StringBuilder("Validation errors occurred:");

                foreach (var error in errors)
                    stringBuilder.AppendLine($" Field: {error.Field}, Error: {error.Message}");

                var errorResponse = new ErrorDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = stringBuilder.ToString(),
                };

                return new BadRequestObjectResult(errorResponse);
            };
        });

        // Add swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Soccer Simulation API",
                Description = "A simple soccer groups simulation API",
                Version = "v1"
            });
        });

        var app = builder.Build();

        // Register middleware
        app.UseMiddleware<ErrorHandlingMiddleware>();

        // Enable static files for serving html
        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}