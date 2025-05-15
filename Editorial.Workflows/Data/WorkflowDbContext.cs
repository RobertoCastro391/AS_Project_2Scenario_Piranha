using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Editorial.Workflows.Models;
using EditorialCMS.Models;

namespace Editorial.Workflows.Data
{
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options) { }

        public DbSet<Workflow> Workflows => Set<Workflow>();
        public DbSet<WorkflowStage> WorkflowStages => Set<WorkflowStage>();
        public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    };
    
    public DbSet<ContentWorkflowBinding> ContentBindings => Set<ContentWorkflowBinding>();
}
