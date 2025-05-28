using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Models;

namespace Piranha.Editorial.Data
{
    public static class EditorialDbExtension
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostEditorialStatus>();
            modelBuilder.Entity<Workflow>();
            modelBuilder.Entity<WorkflowStage>();
            modelBuilder.Entity<WorkflowTransition>();
            modelBuilder.Entity<ContentStateHistory>();
        }
    }
}
