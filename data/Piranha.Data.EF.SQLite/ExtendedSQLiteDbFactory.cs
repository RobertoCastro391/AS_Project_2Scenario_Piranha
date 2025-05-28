using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Piranha.Data.EF.SQLite
{
    public class ExtendedSQLiteDbFactory : IDesignTimeDbContextFactory<ExtendedSQLiteDb>
    {
        public ExtendedSQLiteDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ExtendedSQLiteDb>();
            optionsBuilder.UseSqlite("Filename=editorial.db"); // apenas para gerar migrations

            return new ExtendedSQLiteDb(optionsBuilder.Options);
        }
    }
}
