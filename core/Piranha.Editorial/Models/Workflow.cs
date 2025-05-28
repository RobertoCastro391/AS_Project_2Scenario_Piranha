using System;
using System.Collections.Generic;

namespace Piranha.Editorial.Models
{
    public class Workflow
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        // A lista de etapas deste workflow
        public List<WorkflowStage> Stages { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
