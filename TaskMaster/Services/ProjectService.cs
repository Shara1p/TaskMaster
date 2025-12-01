
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;
using TaskMaster.Models.DTO;

namespace TaskMaster.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActionResult<ProjectResponse>> GetProjectByIdAsync(int id)
    {
        var project = await _dbContext.Projects
             .Include(p => p.Tasks)
             .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return new BadRequestObjectResult($"Project with id {id} not found");
        }

        return new OkObjectResult(project.Adapt<ProjectResponse>());
    }

    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetAllProjectsAsync()
    {
        var projects = await _dbContext.Projects
                    .Include(p => p.Tasks)
                    .ToListAsync();

        if (projects == null || !projects.Any())
        {
            return new BadRequestObjectResult("No projects found");
        }

        return new OkObjectResult(projects.Adapt<List<ProjectResponse>>());
    }

    public async Task<ActionResult<IEnumerable<TaskItemResponse>>> GetTasksByProjectIdAsync(int projectId)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return new BadRequestObjectResult($"Project with {projectId} not found");
        }

        var tasks = project.Tasks.Adapt<List<TaskItemResponse>>();
        return new OkObjectResult(tasks);
    }

    public async Task<ActionResult<ProjectResponse>> CreateProjectAsync(CreateProjectDto project)
    {
        if (project == null)
        {
            return new BadRequestObjectResult("Project data is null");
        }

        if (await _dbContext.Projects.AnyAsync(p => p.Name == project.Name))
        {
            return new BadRequestObjectResult($"Project with name {project.Name} already exists");
        }

        // var projectEntity = project.Adapt<Project>();
        // projectEntity.Id = 0;                // ignore client id
        // projectEntity.Created = DateTime.UtcNow; 
        // projectEntity.Tasks = null;          // avoid creating related tasks from DTO
        // _dbContext.Projects.Add(projectEntity);
        // await _dbContext.SaveChangesAsync();

        // ask about this 

        var projectEntity = project.Adapt<Project>();
        _dbContext.Projects.Add(projectEntity);
        await _dbContext.SaveChangesAsync();

        var response = projectEntity.Adapt<ProjectResponse>();
        return new OkObjectResult(response);
    }

}