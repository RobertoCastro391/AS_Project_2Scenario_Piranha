using System;
using Piranha.Editorial.Enums;

namespace Piranha.Editorial.Models
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
