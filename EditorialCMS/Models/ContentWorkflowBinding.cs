using Editorial.Workflows.Models;

namespace EditorialCMS.Models
{
    public class ContentWorkflowBinding
    {
        public Guid Id { get; set; }

        public Guid WorkflowId { get; set; }
        public Workflow Workflow { get; set; } = null!;

        public Guid CurrentStageId { get; set; }
        public WorkflowStage CurrentStage { get; set; } = null!;

        public string ContentType { get; set; } = string.Empty; // ex: "Page", "Post"
        public string ContentId { get; set; } = string.Empty;   // page/post ID associated with this workflow
    }
}
