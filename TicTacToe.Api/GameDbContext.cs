using Microsoft.EntityFrameworkCore;

public class GameDbContext : DbContext
{
    public DbSet<GameRecord> GameRecords{get;set;}
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
        
    }
}