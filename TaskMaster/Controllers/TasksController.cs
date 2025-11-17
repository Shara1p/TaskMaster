using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;
using TaskMaster.Models.DTO;

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
    public async Task<ActionResult<IEnumerable<TaskItemResponse>>> GetAllTasks()
    {
        var tasks =  await _dbContext.Tasks.ToListAsync();
        return tasks.Adapt<List<TaskItemResponse>>();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemResponse>> GetTaskById(int id)
    {
        var task = await _dbContext.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        return task.Adapt<TaskItemResponse>();
    }

    [HttpGet("{id}/project")]
    public async Task<ActionResult<ProjectResponse>> GetProjectForTask(int id)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return NotFound();
        }

        if (task.Project == null)
        {
            return NotFound("Project not found or task doesn't link to any project");
        }
        return task.Project.Adapt<ProjectResponse>();
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask([FromBody] CreateTaskItemDto task)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        bool taskExists = await _dbContext.Tasks
            .AnyAsync(t => t.ProjectId == task.ProjectId && 
                           t.Title.ToLower() == task.Title.ToLower());
    
        if (taskExists)
        {
            return BadRequest($"Task '{task.Title}' already exists in this project");
        }

        if (!await _dbContext.Projects.AnyAsync(p => p.Id == task.ProjectId))
        {
            return BadRequest($"Project with ID {task.ProjectId} does not exist");
        }

        var newTask = new TaskItem
        {
            Title = task.Title,
            Description = task.Description,
            Created = DateTime.Now,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId
        };

        _dbContext.Add(newTask);
        await _dbContext.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, newTask.Adapt<TaskItemResponse>()); 
    }
}