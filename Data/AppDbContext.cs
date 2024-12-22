using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyFirstApp.Data
{
    /// <summary>
    /// Database context class that manages Entity Framework Core functionality
    /// </summary>
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

    /// <summary>
    /// Model class representing a Todo item with the following properties:
    /// - Id: Unique identifier
    /// - UserId: ID of user who owns the todo
    /// - Title: Description of the todo item
    /// - Completed: Status of the todo
    /// </summary>
    public class Todo
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool Completed { get; set; }

        public override string ToString()
        {
            return $"Todo #{Id}: {Title} (User: {UserId}) - {(Completed ? "Completed" : "Pending")}";
        }
    }

    /// <summary>
    /// Model class representing a User with basic profile information:
    /// - id: Unique identifier
    /// - name: Full name of the user
    /// - username: User's login name
    /// - email: User's email address
    /// </summary>
    public class User
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
    }
}
