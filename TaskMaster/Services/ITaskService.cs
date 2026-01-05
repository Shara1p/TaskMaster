using TaskMaster.Models;
using TaskMaster.Models.Enums;

namespace TaskMaster.Services;
public interface ITaskService
{
    Task<TaskOperationResult> GetTaskByIdAsync(int id);
    Task<IEnumerable<TaskItem>?> GetAllTasksAsync();
    Task<Project?> GetProjectByTaskAsync(int id);
    Task<TaskOperationResult> CreateTaskAsync(TaskItem task);
    Task<TaskOperationResult> UpdateTaskStatusAsync(int taskId, TaskItemStatus newStatus);
    Task<TaskOperationResult> AssignTaskAsync(int taskId, int projectId);
    Task<TaskOperationResult> DeleteTaskAsync(int taskId);
}
