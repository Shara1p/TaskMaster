using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;

namespace TaskMaster.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ApplicationDbContext dbContext, ILogger<ProjectService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        var project = await _dbContext.Projects
             .Include(p => p.Tasks)
             .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return null;

        _logger.LogInformation("Retrieved project with ID {ProjectId}", id);

        return project;
    }

    public async Task<IEnumerable<Project>?> GetAllProjectsAsync()
    {
        var projects = await _dbContext.Projects
                    .Include(p => p.Tasks)
                    .ToListAsync();

        if (projects == null || !projects.Any()) return null;

        _logger.LogInformation("Retrieved {ProjectCount} projects", projects.Count);

        return projects;
    }

    public async Task<ProjectOperationResult> GetTasksByProjectIdAsync(int projectId)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Tasks)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            _logger.LogWarning("Project with ID {ProjectId} not found when retrieving tasks", projectId);
            return new ProjectOperationResult { Project = null, Success = false, ProjectNotFound = true };
        }

        if (project.Tasks == null || !project.Tasks.Any())
        {
            _logger.LogInformation("No tasks found for project with ID {ProjectId}", projectId);
            return new ProjectOperationResult { Project = project, Success = true, ProjectNotFound = false, HasTasks = false };
        }

        _logger.LogInformation("Retrieved {TaskCount} tasks for project with ID {ProjectId}", project.Tasks.Count, projectId);
        return new ProjectOperationResult
        { Project = project, Success = true, ProjectNotFound = false, HasTasks = true };
    }

    public async Task<ProjectOperationResult> CreateProjectAsync(Project project)
    {
        if (project == null)
        {
            _logger.LogWarning("Attempted to create a project with null data");
            return new ProjectOperationResult { Project = null, Success = false };
        }

        if (await _dbContext.Projects.AnyAsync(p => p.Name == project.Name))
        {
            _logger.LogWarning("A project with the name '{ProjectName}' already exists", project.Name);
            return new ProjectOperationResult { Project = null, Success = false, ProjectExists = true };
        }

        project.Created = DateTime.UtcNow;

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully created new project with ID {ProjectId}", project.Id);

        return new ProjectOperationResult { Project = project, Success = true, ProjectNotFound = false };
    }

}
