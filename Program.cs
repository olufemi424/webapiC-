/*
 * This program demonstrates a simple C# application that fetches todo items from a REST API.
 *
 * Key features:
 * - Makes HTTP requests to JSONPlaceholder API (https://jsonplaceholder.typicode.com/todos)
 * - Deserializes JSON response into strongly-typed Todo objects
 * - Displays the first 5 todo items with their details (ID, title, user ID, completion status)
 * - Demonstrates async/await pattern for HTTP calls
 * - Shows basic array manipulation and generic method usage
 * - Implements custom ToString() for better object representation
 *
 * Note: The Main method uses .Result which should be avoided in production code.
 * Instead, use async Main for proper asynchronous execution.
 */

using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using DotNetEnv;
using Microsoft.Extensions.Logging;
using MyFirstApp.Data;
using MyFirstApp.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;

internal class Program
{
    static void Main(string[] args)
    {
        DotNetEnv.Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        // Configure environment variables based on environment
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddScoped<DatabaseInitializationService>();
        builder.Services.AddHttpClient();

        // Configure PostgreSQL using environment variables
        var requiredEnvVars = new Dictionary<string, string?>
        {
            { "DB_HOST", Environment.GetEnvironmentVariable("DB_HOST") },
            { "DB_PORT", Environment.GetEnvironmentVariable("DB_PORT") },
            { "DB_NAME", Environment.GetEnvironmentVariable("DB_NAME") },
            { "DB_USER", Environment.GetEnvironmentVariable("DB_USER") },
            { "DB_PASSWORD", Environment.GetEnvironmentVariable("DB_PASSWORD") },
            { "DB_SCHEMA", Environment.GetEnvironmentVariable("DB_SCHEMA") }
        };

        // Check for missing environment variables
        var missingVars = requiredEnvVars
            .Where(var => string.IsNullOrEmpty(var.Value))
            .Select(var => var.Key)
            .ToList();

        if (missingVars.Any())
        {
            throw new InvalidOperationException(
                $"Missing required environment variables: {string.Join(", ", missingVars)}. " +
                "Please ensure all required environment variables are set in the .env file."
            );
        }

        var connectionString = $"Host={requiredEnvVars["DB_HOST"]};" +
                             $"Port={requiredEnvVars["DB_PORT"]};" +
                             $"Database={requiredEnvVars["DB_NAME"]};" +
                             $"Username={requiredEnvVars["DB_USER"]};" +
                             $"Password={requiredEnvVars["DB_PASSWORD"]};" +
                             $"SearchPath={requiredEnvVars["DB_SCHEMA"]}";

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            if (builder.Environment.IsDevelopment())
            {
                options.EnableDetailedErrors()
                       .LogTo(Console.WriteLine,
                             LogLevel.Warning,
                             DbContextLoggerOptions.None);
            }
        });

        var app = builder.Build();

        // Initialize database
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbInitService = scope.ServiceProvider.GetRequiredService<DatabaseInitializationService>();

            try
            {
                if (app.Environment.IsDevelopment())
                {
                    db.Database.EnsureCreated();
                }
                dbInitService.InitializeAsync().Wait();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        // Health check endpoint
        app.MapGet("/health", () => new {
            status = "healthy",
            database = "connected",
            application = "running",
            timestamp = DateTime.UtcNow
        });

        // Endpoint to fetch todos from JSONPlaceholder
        app.MapGet("/todos", async (IHttpClientFactory clientFactory, ILogger<Program> logger) =>
        {
            try
            {
                var client = clientFactory.CreateClient();
                var response = await client.GetAsync("https://jsonplaceholder.typicode.com/todos");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Todo[]>(json) ?? Array.Empty<Todo>();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Error fetching todos from JSONPlaceholder API");
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Error deserializing todos response");
                throw;
            }
        });

        // Endpoint to get users with optional count parameter
        app.MapGet("/users", async (bool? count, IHttpClientFactory clientFactory, ILogger<Program> logger) =>
        {
            try
            {
                var client = clientFactory.CreateClient();
                var response = await client.GetAsync("https://jsonplaceholder.typicode.com/users");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<User[]>(json) ?? Array.Empty<User>();

                // Return count only if the query parameter is present
                return count == true
                    ? Results.Ok(new { count = users.Length, users = users })
                    : Results.Ok(new { users = users });
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Error fetching users from JSONPlaceholder API");
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Error deserializing users response");
                throw;
            }
        });

        app.Run();
    }
}
