# MyFirstApp - C# Web API with PostgreSQL

A modern C# Web API that demonstrates integration with PostgreSQL and external APIs, featuring environment-based configuration, health monitoring, and structured logging.

## Features

- 🔐 Environment-based configuration
- 📊 PostgreSQL database integration
- 🌐 External API integration (JSONPlaceholder)
- 🏥 Health monitoring endpoint
- 📝 Structured logging
- 🔍 Schema-based database organization

## Quick Start

### Prerequisites

- .NET SDK 9.0+
- PostgreSQL 14+
- Git

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd MyFirstApp
```

2. Create environment file:
```bash
cp .env.example .env
# Edit .env with your database credentials
```

3. Set up the database:
```bash
# Connect to PostgreSQL
psql -U postgres

# Create database and user
CREATE USER boilerplate_app_user WITH PASSWORD 'Password';
CREATE DATABASE boilerplatedb OWNER boilerplate_app_user;

# Create schema
\c boilerplatedb
CREATE SCHEMA boilerplate AUTHORIZATION boilerplate_app_user;
```

4. Install dependencies:
```bash
dotnet restore
```

### Running the Application

Development mode:
```bash
dotnet run --environment Development
```

The API will be available at `http://localhost:7000`

### API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/health` | GET | System health status |
| `/todos` | GET | Fetch todos from JSONPlaceholder |
| `/users` | GET | Fetch users with optional count |

Query Parameters:
- `/users?count=true` - Include total count with users

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| DB_HOST | Database host | localhost |
| DB_PORT | Database port | 5432 |
| DB_USER | Database username | boilerplate_app_user |
| DB_NAME | Database name | boilerplatedb |
| DB_SCHEMA | Database schema | boilerplate |
| DB_LOG_ENABLED | Enable database logging | true |

## Project Structure

```
MyFirstApp/
├── Data/
│   └── AppDbContext.cs     # Database context and models
├── Services/
│   └── DatabaseInitializationService.cs
├── Properties/
│   └── launchSettings.json # Environment settings
├── Program.cs             # Application entry point
├── appsettings.json      # Base configuration
├── appsettings.Development.json
├── .env                  # Environment variables
└── README.md
```

## Development

### Adding New Endpoints

Example of adding a new endpoint:

```csharp
app.MapGet("/custom", () => new { message = "Custom endpoint" });
```

### Database Migrations

```bash
# Create a migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update
```

## Testing

Using curl:
```bash
# Health check
curl http://localhost:7000/health

# Get todos
curl http://localhost:7000/todos

# Get users with count
curl http://localhost:7000/users?count=true
```

## Logging

Logs are configured in `appsettings.Development.json` for development environment:
- Default level: Information
- Database operations: Warning
- Framework logs: Warning

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Troubleshooting

Common issues and solutions:

1. **Database Connection Failed**
   - Verify PostgreSQL is running
   - Check credentials in .env
   - Ensure database exists

2. **Schema Not Found**
   - Run schema creation SQL
   - Check schema name in .env

3. **Port Already in Use**
   - Change port in launchSettings.json
   - Kill process using the port

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Additional Resources

- [Detailed Tutorial](TUTORIAL.md)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
