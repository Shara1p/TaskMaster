using Microsoft.AspNetCore.Mvc;
using TaskMaster.Models.DTO;
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
        return await _taskService.GetAllTasksAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemResponse>> GetTaskById(int id)
    {
        return await _taskService.GetTaskByIdAsync(id);
    }

    [HttpGet("{id}/project")]
    public async Task<ActionResult<ProjectResponse>> GetProjectByTask(int id)
    {
        return await _taskService.GetProjectByTaskAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemResponse>> CreateTask([FromBody] CreateTaskItemDto task)
    {
        return await _taskService.CreateTaskAsync(task);
    }

    [HttpPatch("{taskId}/status")]
    public async Task<ActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskStatusDto newStatus)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _taskService.UpdateTaskStatusAsync(taskId, newStatus.Status!.Value);
        return result.IsSuccess
            ? NoContent()           
            : BadRequest(result.ErrorMessage); 
    }
}
