namespace Piranha.Editorial.Interfaces
{
    public interface IEditorialWorkflowService
    {
        Task EnsurePageStatusAsync(Guid pageId);
    }
}
