using Microsoft.AspNetCore.Mvc;
using TaskMaster.Models.DTO;

namespace TaskMaster.Services;
public interface IProjectService
{
    Task<ActionResult<ProjectResponse>> GetProjectByIdAsync(int id);
    Task<ActionResult<IEnumerable<ProjectResponse>>> GetAllProjectsAsync();
    Task<ActionResult<IEnumerable<TaskItemResponse>>> GetTasksByProjectIdAsync(int projectId);
    Task<ActionResult<ProjectResponse>> CreateProjectAsync(CreateProjectDto project);
}
