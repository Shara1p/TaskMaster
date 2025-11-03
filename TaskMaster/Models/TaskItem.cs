using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMaster.Models;

public class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime Created { get; set; }
    public DateTime? DueDate { get; set; }

    [ForeignKey("ProjectId")]
    public required int ProjectId { get; set; }

    public required Project Project { get; set; }
}
