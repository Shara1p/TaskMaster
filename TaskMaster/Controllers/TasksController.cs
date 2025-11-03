using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;

namespace TaskMaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public TasksController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks()
    {
        return await _dbContext.Tasks.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskById(int id)
    {
        var task = await _dbContext.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        return task;
    }

    [HttpGet("{id}/project")] // Логично - проект конкретной задачи
    public async Task<ActionResult<Project>> GetProjectForTask(int id)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return NotFound();
        }

        return task.Project;
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask([FromBody] TaskItem task)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (! await _dbContext.Projects.AnyAsync(p => p.Id == task.ProjectId))
        {
            return BadRequest($"Project with ID {task.ProjectId} does not exist");
        }

        if (task.Created == default)
        {
            task.Created = DateTime.Now;
        }

        _dbContext.Add(task);
        await _dbContext.SaveChangesAsync();
        
        var createdTask = await _dbContext.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == task.Id);
        
        
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, createdTask); 
    }
}