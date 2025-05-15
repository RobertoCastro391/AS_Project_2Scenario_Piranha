using Editorial.Workflows.Data;
using Editorial.Workflows.Models;
using EditorialCMS.Models;
using EditorialCMS.Services;
using Microsoft.EntityFrameworkCore;

namespace Editorial.Workflows.Services;

public class WorkflowService : IWorkflowService
{
    private readonly WorkflowDbContext _db;

    public WorkflowService(WorkflowDbContext db)
    {
        _db = db;
    }

    public async Task BindContentToWorkflowAsync(string contentType, string contentId, Guid workflowId)
    {
        var workflow = await _db.Workflows
            .Include(w => w.Stages.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        if (workflow == null)
            throw new InvalidOperationException("Workflow not found.");

        var firstStage = workflow.Stages.OrderBy(s => s.Order).FirstOrDefault();

        if (firstStage == null)
            throw new InvalidOperationException("Workflow has no stages.");

        var exists = await _db.ContentBindings
            .AnyAsync(b => b.ContentId == contentId && b.ContentType == contentType);

        if (exists)
            throw new InvalidOperationException("Content is already bound to a workflow.");

        var binding = new ContentWorkflowBinding
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflow.Id,
            CurrentStageId = firstStage.Id,
            ContentType = contentType,
            ContentId = contentId
        };

        _db.ContentBindings.Add(binding);
        await _db.SaveChangesAsync();
    }
}