using Kaida.AuthServer.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaida.AuthServer.Data;

public class AuthServerDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source = AuthServer.db");
    }
}

