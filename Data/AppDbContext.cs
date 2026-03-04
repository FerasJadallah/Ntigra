using Microsoft.EntityFrameworkCore;
using Ntigra.Models;

namespace Ntigra.Data;

// This class represents the database context for the application, allowing us to interact with the database using Entity Framework Core.

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
}
