using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyFirstApp.Data;

namespace MyFirstApp.Services
{
    /// <summary>
    /// Service responsible for database initialization and validation tasks.
    /// This service connects to the database, verifies the connection,
    /// and logs information about available database tables.
    ///
    /// Key responsibilities:
    /// - Validates database connectivity
    /// - Queries and logs all table names in the public schema
    /// - Provides detailed logging of database connection status
    /// - Handles and logs any database connection errors
    /// </summary>
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
                // Get all table names from the database using the correct schema
                var tableNames = await _context.Database.SqlQuery<string>(
                    $@"SELECT table_name
                       FROM information_schema.tables
                       WHERE table_schema = '{_context.Schema}'")  // Use the public Schema property
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
