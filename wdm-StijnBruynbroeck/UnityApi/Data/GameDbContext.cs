using Microsoft.EntityFrameworkCore;
using UnityApi.Models; 

namespace UnityApi.Data
{
    public class GameDbContext : DbContext
    {
        public DbSet<GameAction> GameActions { get; set; }

        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite("Data Source=GameData.db");
            }
        }
    }
}