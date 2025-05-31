using System;
using Piranha.Editorial.Abstractions.Enums;

namespace Piranha.Editorial.Abstractions.Models
{
    public class PageEditorialStatus
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public Guid WorkflowId { get; set; }
        public Guid CurrentStageId { get; set; }
        public EditorialStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
