using Microsoft.AspNetCore.Mvc;
using TaskMaster.Models.DTO;
using TaskMaster.Services;

namespace TaskMaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;   

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetAllProjects()
    {
        return await _projectService.GetAllProjectsAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResponse>> GetProject(int id)
    {
        return await _projectService.GetProjectByIdAsync(id);
    }

    [HttpGet("{id}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskItemResponse>>> GetTasks(int id)
    {
        return await _projectService.GetTasksByProjectIdAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> CreateProject(CreateProjectDto project)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }
        return await _projectService.CreateProjectAsync(project);
    }
}