using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteServer.Models
{
    internal class AppDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ConnectionHistory> Histories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=remote.db");  // File DB
        }
    }
}
