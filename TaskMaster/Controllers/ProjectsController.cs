using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;
using TaskMaster.Models.DTO;

namespace TaskMaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetAllProjects()
    {
        var projects = await _dbContext.Projects
            .Include(p => p.Tasks)
            .ToListAsync();
        return projects.Adapt<List<ProjectResponse>>();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResponse>> GetProject(int id)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return NotFound();
        }

        return project.Adapt<ProjectResponse>();
    }

    [HttpGet("{id}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskItemResponse>>> GetTasks(int id)
    {
        if (!await _dbContext.Projects.AnyAsync(p => p.Id == id))
        {
            return NotFound();
        }

        var tasks = await _dbContext.Tasks.Where(t => t.ProjectId == id).ToListAsync();
        return tasks.Adapt<List<TaskItemResponse>>();
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(CreateProjectDto project)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (await _dbContext.Projects.AnyAsync(p => p.Name == project.Name))
        {
            return BadRequest($"Project with name {project.Name} already exists");
        }

        var newProject = new Project
        {
            Name = project.Name,
            Description = project.Description,
            Created = DateTime.Now
        };

        _dbContext.Add(newProject);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProject), new { id = newProject.Id }, newProject.Adapt<ProjectResponse>());
    }
}