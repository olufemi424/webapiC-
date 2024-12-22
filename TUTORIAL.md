# Building a C# Web API with PostgreSQL and External API Integration

This tutorial will guide you through creating a C# Web API that connects to a PostgreSQL database and integrates with an external API (JSONPlaceholder). You'll learn about environment configuration, database connectivity, dependency injection, and API endpoints.

## Project Overview

This project demonstrates:
- Setting up a .NET Web API
- PostgreSQL database integration using Entity Framework Core
- Environment variable configuration
- External API integration
- Error handling and logging
- Health checks
- Database schema management

## Prerequisites

- .NET SDK (version 9.0 or later)
- PostgreSQL database
- Basic understanding of C# and REST APIs
- A code editor (VS Code recommended)

## Step 1: Create the Project Structure

First, create a new web API project:
dotnet new web -n MyFirstApp
cd MyFirstApp

Add required NuGet packages
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package DotNetEnv
dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore

## Step 2: Set Up Environment Configuration

Create a `.env` file in your project root:

```plaintext
DB_HOST=localhost
DB_PORT=5432
DB_USER=boilerplate_app_user
DB_PASSWORD=Password
DB_NAME=boilerplatedb
DB_SCHEMA=boilerplate
DB_LOG_ENABLED=true
```

Create `appsettings.Development.json` for development-specific settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
      "Microsoft.Extensions.Hosting": "Information",
      "MyFirstApp": "Information"
    }
  },
  "DetailedErrors": true
}
```

## Step 3: Create Database Models

Create `Data/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyFirstApp.Data
{
    public class AppDbContext : DbContext
    {
        private readonly string _schema;
        public string Schema => _schema;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            _schema = Environment.GetEnvironmentVariable("DB_SCHEMA") ?? "boilerplate";
        }

        public DbSet<Todo> Todos { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_schema);
            modelBuilder.Entity<Todo>()
                .Property(t => t.Title)
                .IsRequired();
        }
    }

    public class Todo
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool Completed { get; set; }
    }

    public class User
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
    }
}
```

## Step 4: Create Database Initialization Service

Create `Services/DatabaseInitializationService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyFirstApp.Data;

namespace MyFirstApp.Services
{
    public class DatabaseInitializationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseInitializationService> _logger;

        public DatabaseInitializationService(AppDbContext context, ILogger<DatabaseInitializationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var tableNames = await _context.Database.SqlQuery<string>(
                    $@"SELECT table_name
                       FROM information_schema.tables
                       WHERE table_schema = '{_context.Schema}'")
                    .ToListAsync();

                _logger.LogInformation("Connected to database successfully!");
                _logger.LogInformation($"Available tables in schema '{_context.Schema}':");
                foreach (var tableName in tableNames)
                {
                    _logger.LogInformation($"- {tableName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while connecting to database");
                throw;
            }
        }
    }
}
```

## Step 5: Configure API Endpoints

The main Program.cs file sets up the API endpoints and configures services:

```csharp
// Health check endpoint
app.MapGet("/health", () => new {
    status = "healthy",
    database = "connected",
    application = "running",
    timestamp = DateTime.UtcNow
});

// Fetch todos from JSONPlaceholder
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
    catch (Exception ex)
    {
        logger.LogError(ex, "Error fetching todos");
        throw;
    }
});

// Get users with optional count
app.MapGet("/users", async (bool? count, IHttpClientFactory clientFactory, ILogger<Program> logger) =>
{
    try
    {
        var client = clientFactory.CreateClient();
        var response = await client.GetAsync("https://jsonplaceholder.typicode.com/users");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<User[]>(json) ?? Array.Empty<User>();

        return count == true
            ? Results.Ok(new { count = users.Length, users = users })
            : Results.Ok(new { users = users });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error fetching users");
        throw;
    }
});
```

## Step 6: Run the Application

```bash
# Create the database (if it doesn't exist)
createdb -U boilerplate_app_user boilerplatedb

# Run the application
dotnet run --environment Development
```

## Testing the API

Use curl or Postman to test the endpoints:

```bash
# Health check
curl http://localhost:7000/health

# Get todos
curl http://localhost:7000/todos

# Get users with count
curl http://localhost:7000/users?count=true
```

## Key Features Explained

1. **Environment Configuration**
   - Uses .env file for sensitive data
   - Different settings for development/production
   - Environment variable validation

2. **Database Integration**
   - PostgreSQL connection with schema support
   - Entity Framework Core for data access
   - Automatic table creation
   - Schema validation on startup

3. **External API Integration**
   - HTTPClient Factory for API calls
   - JSON deserialization
   - Error handling and logging

4. **Logging**
   - Structured logging
   - Development vs production logging levels
   - Database operation logging

## Best Practices Demonstrated

- Dependency Injection
- Environment-based configuration
- Error handling and logging
- Schema-based database organization
- API documentation
- Health checks
- Secure credential management

## Next Steps

1. Add authentication
2. Implement CRUD operations
3. Add request validation
4. Implement caching
5. Add API documentation using Swagger
6. Add unit tests
7. Set up CI/CD pipeline

## Common Issues and Solutions

1. **Database Connection Issues**
   - Verify PostgreSQL is running
   - Check credentials in .env file
   - Ensure database and schema exist

2. **API Errors**
   - Check logs for detailed error messages
   - Verify external API availability
   - Check network connectivity

3. **Environment Issues**
   - Verify .env file exists and is properly formatted
   - Check environment variable names
   - Ensure proper file permissions
```

This tutorial provides a comprehensive guide for beginners while also including advanced concepts and best practices. It can be expanded based on specific areas where you'd like more detail.
