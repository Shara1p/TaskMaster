using Mapster;
using Microsoft.AspNetCore.Mvc;
using TaskMaster.Models;
using TaskMaster.Models.DTO;
using TaskMaster.Models.Enums;
using TaskMaster.Services;

namespace TaskMaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemResponse>>> GetAllTasks()
    {
        var tasks = await _taskService.GetAllTasksAsync();
        if (tasks == null || !tasks.Any())
            return NotFound("No tasks found.");


        return Ok(tasks.Adapt<IEnumerable<TaskItemResponse>>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemResponse>> GetTaskById(int id)
    {
        var taskResponse = await _taskService.GetTaskByIdAsync(id);
        if (taskResponse.TaskNotFound)
            return NotFound($"Task with ID {id} not found.");
        return Ok(taskResponse.Task.Adapt<TaskItemResponse>());
    }

    [HttpGet("{id}/project")]
    public async Task<ActionResult<ProjectResponse>> GetProjectByTask(int id)
    {
        var projectResponse = await _taskService.GetProjectByTaskAsync(id);
        if (projectResponse == null)
            return NotFound($"Project for Task with ID {id} not found.");
        return Ok(projectResponse.Adapt<ProjectResponse>());
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemResponse>> CreateTask([FromBody] CreateTaskItemDto task)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var newTask = new TaskItem
        {
            Title = task.Title!,
            Description = task.Description,
            Status = Enum.TryParse<TaskItemStatus>(task.Status, true, out var s) ? s : TaskItemStatus.Analysis,
            Created = DateTime.UtcNow,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId
        };

        var result = await _taskService.CreateTaskAsync(newTask);
        if (!result.Success)
        {
            if (result.TaskExists)
                return Conflict("A task with the same title already exists in the specified project.");
            if (result.TaskNotFound)
                return NotFound("The specified project does not exist.");
            return BadRequest("Invalid task data.");
        }
        return CreatedAtAction(nameof(GetTaskById), new { id = result.Task!.Id }, result.Task.Adapt<TaskItemResponse>());
    }

    [HttpPatch("{taskId}/status")]
    public async Task<ActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskStatusDto newStatus)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _taskService.UpdateTaskStatusAsync(taskId, newStatus.Status!.Value);

        if (!result.Success)
        {
            if (result.TaskNotFound)
                return NotFound($"Task with ID {taskId} not found.");
            if (result.InvalidTransition)
                return BadRequest("Invalid transition.");
            return BadRequest("Failed to update task status.");
        } 
        
        return NoContent();
    }
}
