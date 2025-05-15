using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editorial.Workflows.Models
{
    public class Workflow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<WorkflowStage> Stages { get; set; } = new List<WorkflowStage>();
    }

    public class WorkflowStage
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }

        public ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
    }

    public class WorkflowTransition
    {
        public Guid Id { get; set; }
        public Guid FromStageId { get; set; }
        public Guid ToStageId { get; set; }
        public string Condition { get; set; } = string.Empty; // To use in the futurue if needed
    }
}