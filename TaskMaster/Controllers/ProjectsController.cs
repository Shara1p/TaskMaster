using Mapster;
using Microsoft.AspNetCore.Mvc;
using TaskMaster.Models;
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
    public async Task<ActionResult<IEnumerable<Project>?>> GetAllProjects()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        if (projects == null)
        {
            return NoContent();
        }
        return Ok(projects.Adapt<IEnumerable<ProjectResponse>>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound($"Project with ID {id} not found.");
        }
        return Ok(project.Adapt<ProjectResponse>());
    }

    [HttpGet("{id}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByProjectId(int id)
    {
        var result = await _projectService.GetTasksByProjectIdAsync(id);

        if (!result.Success)
        {
            if (result.ProjectNotFound)
                return NotFound($"Project with ID {id} not found.");
            return BadRequest("An error occurred while retrieving tasks.");
        }
        
        if (!result.HasTasks)
            return NotFound($"No tasks found for Project with ID {id}.");
        return Ok(result.Project.Tasks.Adapt<IEnumerable<TaskItemResponse>>());
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> CreateProject(CreateProjectDto project)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }
        var newProject = new Project
        {
            Name = project.Name!,
            Description = project.Description,
            Created = DateTime.UtcNow,
        };
        
        var result = await _projectService.CreateProjectAsync(newProject);
        if (!result.Success)
        {
            if (result.ProjectExists)
                return Conflict($"Project with name '{project.Name}' already exists.");
            return BadRequest("An error occurred while creating the project.");
        }
        return CreatedAtAction(nameof(GetProject), new { id = result.Project!.Id }, result.Project.Adapt<ProjectResponse>());
    }
}
