using Piranha.Editorial.Abstractions.Models;

namespace Piranha.Editorial.Interfaces
{
    public interface IEditorialWorkflowService
    {
        Task EnsurePageStatusAsync(Guid pageId);
        Task<PageEditorialStatusDto?> GetStatusForPageAsync(Guid pageId);

    }
}
