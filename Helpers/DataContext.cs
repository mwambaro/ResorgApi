using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ResorgApi.Entities;

namespace ResorgApi.Helpers
{
    public class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        private readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Database provider. We use SQLite for now.
        /// </summary>
        /// <param name="options"></param>
        /// To use a different database (e.g. SQL Server, MySql, PostgreSQL) update the 
        /// database provider in this method then delete and regenerate the database 
        /// migrations with the command `dotnet ef migrations add InitialCreate`. 
        /// Database migrations are run on startup so the database is created automatically 
        /// the first time you start the api.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sqlite database
            options.UseSqlite(Configuration.GetConnectionString("ResorgApiDatabase"));
        }
    }
}
