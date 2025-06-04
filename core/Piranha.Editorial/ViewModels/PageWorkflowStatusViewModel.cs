using Piranha.Editorial.Abstractions.Models;

namespace Piranha.Editorial.ViewModels
{
    public class PageWorkflowStatusViewModel
    {
        public Guid PageId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public string StatusCMS { get; set; } // ex: Draft, Published
        public string WorkflowStage { get; set; } // ex: Em revisão, Aprovado
        public string SiteName { get; set; }
        public List<ContentStateHistory> History { get; set; } = new();

    }
}