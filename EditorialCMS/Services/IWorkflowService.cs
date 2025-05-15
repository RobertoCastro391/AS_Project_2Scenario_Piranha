namespace EditorialCMS.Services
{
    public interface IWorkflowService
    {        
        Task BindContentToWorkflowAsync(string contentType, string contentId, Guid workflowId);
    }
}
