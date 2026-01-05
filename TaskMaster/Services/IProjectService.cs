using TaskMaster.Models;

namespace TaskMaster.Services;

public interface IProjectService
{
    Task<Project?> GetProjectByIdAsync(int id);
    Task<IEnumerable<Project>?> GetAllProjectsAsync();
    Task<ProjectOperationResult> GetTasksByProjectIdAsync(int projectId);
    Task<ProjectOperationResult> CreateProjectAsync(Project project);
}
