using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Abstractions.Models;

namespace Piranha.Editorial.Data
{
    public static class EditorialDbExtension
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PageEditorialStatus>();
            modelBuilder.Entity<Workflow>();
            modelBuilder.Entity<WorkflowStage>();
            modelBuilder.Entity<WorkflowTransition>();
            modelBuilder.Entity<ContentStateHistory>();
        }
    }
}
