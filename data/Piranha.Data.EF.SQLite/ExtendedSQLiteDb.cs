using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Models;

namespace Piranha.Data.EF.SQLite
{
    public class ExtendedSQLiteDb : DbContext
    {
        public ExtendedSQLiteDb(DbContextOptions<ExtendedSQLiteDb> options) : base(options) { }

        public DbSet<PostEditorialStatus> PostEditorialStatuses { get; set; }
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowStage> WorkflowStages { get; set; }
        public DbSet<WorkflowTransition> WorkflowTransitions { get; set; }
        public DbSet<ContentStateHistory> ContentStateHistories { get; set; }
    }
}
