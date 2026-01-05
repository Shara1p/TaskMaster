using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;

namespace TaskMaster.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        var project = await _dbContext.Projects
             .Include(p => p.Tasks)
             .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return null;

        return project;
    }

    public async Task<IEnumerable<Project>?> GetAllProjectsAsync()
    {
        var projects = await _dbContext.Projects
                    .Include(p => p.Tasks)
                    .ToListAsync();

        if (projects == null || !projects.Any()) return null;

        return projects;
    }

    public async Task<ProjectOperationResult> GetTasksByProjectIdAsync(int projectId)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Tasks)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) 
            return new ProjectOperationResult { Project = null, Success = false, ProjectNotFound = true };

        if (project.Tasks == null || !project.Tasks.Any()) return new ProjectOperationResult 
            { Project = project, Success = true, ProjectNotFound = false, HasTasks = false };

        return new ProjectOperationResult 
            { Project = project, Success = true, ProjectNotFound = false, HasTasks = true };
    }

    public async Task<ProjectOperationResult> CreateProjectAsync(Project project)
    {
        if (project == null)
        {
            return new ProjectOperationResult { Project = null, Success = false};
        }

        if (await _dbContext.Projects.AnyAsync(p => p.Name == project.Name))
        {
            return new ProjectOperationResult { Project = null, Success = false, ProjectExists = true };
        }

        project.Created = DateTime.UtcNow;

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        return new ProjectOperationResult { Project = project, Success = true, ProjectNotFound = false };
    }

}
