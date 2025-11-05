using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;

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
    public async Task<ActionResult<IEnumerable<Project>>> GetAllProjects()
    {
        return await _dbContext.Projects.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        var project = await _dbContext.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        return project;
    }

    [HttpGet("{id}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks(int id)
    {
        if (!await _dbContext.Projects.AnyAsync(p => p.Id == id))
        {
            return NotFound();
        }

        return await _dbContext.Tasks.Where(t => t.ProjectId == id).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (await _dbContext.Projects.AnyAsync(p => p.Name == project.Name))
        {
            return BadRequest($"Project with name {project.Name} already exists");
        }

        if (project.Created == default)
        {
            project.Created = DateTime.Now;
        }

        _dbContext.Add(project);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(CreateProject), new { id = project.Id }, project);
    }
}